﻿using System.Collections.Concurrent;
using System.Reflection;

namespace LeoDB;

public partial class BsonMapper
{
    /// <summary>
    /// Mapping cache between Class/BsonDocument
    /// </summary>
    private readonly ConcurrentDictionary<Type, EntityMapper> _entities = new();

    /// <summary>
    /// Get property mapper between typed .NET class and BsonDocument - Cache results
    /// </summary>
    internal EntityMapper GetEntityMapper(Type type)
    {
        if (_entities.TryGetValue(type, out EntityMapper mapper))
        {
            return mapper;
        }

        using var cts = new CancellationTokenSource();
        try
        {
            // We need to add the empty shell, because ``BuildEntityMapper`` may use this method recursively
            var newMapper = new EntityMapper(type, cts.Token);
            mapper = _entities.GetOrAdd(type, newMapper);
            if (ReferenceEquals(mapper, newMapper))
            {
                try
                {
                    this.BuildEntityMapper(mapper);
                }
                catch (Exception ex)
                {
                    _entities.TryRemove(type, out _);
                    throw new LeoException(LeoException.MAPPING_ERROR, $"Error in '{type.Name}' mapping: {ex.Message}", ex);
                }
            }
        }
        finally
        {
            // Allow the Mapper to be used for de-/serialization
            cts.Cancel();
        }

        return mapper;
    }

    /// <summary>
    /// Use this method to override how your class can be, by default, mapped from entity to Bson document.
    /// Returns an EntityMapper from each requested Type
    /// </summary>
    protected void BuildEntityMapper(EntityMapper mapper)
    {
        var idAttr = typeof(CollectionIdAttribute);
        var ignoreAttr = typeof(CollectionFieldIgnoreAttribute);
        var fieldAttr = typeof(CollectionFieldAttribute);
        var dbrefAttr = typeof(CollectionReferenceAttribute);
        var isUniqueAttr = typeof(CollectionUniqueAttribute);

        var members = this.GetTypeMembers(mapper.ForType);
        var id = this.GetIdMember(members);

        foreach (var memberInfo in members)
        {
            // Validar si es un miembro ignorado.
            if (CustomAttributeExtensions.IsDefined(memberInfo, ignoreAttr, true))
                continue;

            // Obtener el nombre del campo.
            var name = this.ResolveFieldName(memberInfo.Name);

            // Validar si el campo tiene el atributo Field con un nombre de campo personalizado.
            var field = (CollectionFieldAttribute)CustomAttributeExtensions.GetCustomAttributes(memberInfo, fieldAttr, true).FirstOrDefault();

            // Obtener el nombre personalizado en caso de que exista.
            if (field is not null && field.Name is not null)
            {
                name = field.Name;
            }

            // Si es el campo Id, se establece el nombre.
            if (memberInfo == id)
            {
                name = "_id";
            }

            // create getter/setter function
            var getter = Reflection.CreateGenericGetter(mapper.ForType, memberInfo);
            var setter = Reflection.CreateGenericSetter(mapper.ForType, memberInfo);

            // check if property has [BsonId] to get with was setted AutoId = true
            var autoId = (CollectionIdAttribute)CustomAttributeExtensions.GetCustomAttributes(memberInfo, idAttr, true).FirstOrDefault();

            // get data type
            var dataType = memberInfo is PropertyInfo
                ? (memberInfo as PropertyInfo).PropertyType
                : (memberInfo as FieldInfo).FieldType;

            // check if datatype is list/array
            var isEnumerable = Reflection.IsEnumerable(dataType);


            // Validar si el campo tiene el atributo de unico.
            var fieldUnique = (CollectionUniqueAttribute)CustomAttributeExtensions.GetCustomAttributes(memberInfo, isUniqueAttr, true).FirstOrDefault();

            // create a property mapper
            var member = new MemberMapper
            {
                AutoId = autoId == null ? true : autoId.AutoId,
                FieldName = name,
                MemberName = memberInfo.Name,
                DataType = dataType,
                IsEnumerable = isEnumerable,
                UnderlyingType = isEnumerable ? Reflection.GetListItemType(dataType) : dataType,
                Getter = getter,
                Setter = setter,
                IsUnique = fieldUnique is not null
            };

            // check if property has [BsonRef]
            var dbRef = (CollectionReferenceAttribute)CustomAttributeExtensions.GetCustomAttributes(memberInfo, dbrefAttr, false)
                .FirstOrDefault();

            if (dbRef != null && memberInfo is PropertyInfo)
            {
                RegisterDbRef(this, member, _typeNameBinder,
                    dbRef.Collection ?? this.ResolveCollectionName((memberInfo as PropertyInfo).PropertyType));
            }

            // support callback to user modify member mapper
            this.ResolveMember?.Invoke(mapper.ForType, memberInfo, member);

            // test if has name and there is no duplicate field
            // when member is not ignore
            if (member.FieldName != null &&
                mapper.Members.Any(x => x.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase)) == false &&
                !member.IsIgnore)
            {
                mapper.Members.Add(member);
            }
        }
    }

    /// <summary>
    /// Gets MemberInfo that refers to Id from a document object.
    /// </summary>
    protected virtual MemberInfo GetIdMember(IEnumerable<MemberInfo> members)
    {
        return Reflection.SelectMember(members,
            x => CustomAttributeExtensions.IsDefined(x, typeof(CollectionIdAttribute), true),
            x => x.Name.Equals("Id", StringComparison.OrdinalIgnoreCase),
            x => x.Name.Equals(x.DeclaringType.Name + "Id", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns all member that will be have mapper between POCO class to document
    /// </summary>
    protected virtual IEnumerable<MemberInfo> GetTypeMembers(Type type)
    {
        var members = new List<MemberInfo>();

        var flags = this.IncludeNonPublic
            ? (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : (BindingFlags.Public | BindingFlags.Instance);

        members.AddRange(type.GetProperties(flags)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .Select(x => x as MemberInfo));

        if (this.IncludeFields)
        {
            members.AddRange(type.GetFields(flags).Where(x => !x.Name.EndsWith("k__BackingField") && x.IsStatic == false)
                .Select(x => x as MemberInfo));
        }

        return members;
    }

    /// <summary>
    /// Get best construtor to use to initialize this entity.
    /// - Look if contains [BsonCtor] attribute
    /// - Look for parameterless ctor
    /// - Look for first contructor with parameter and use BsonDocument to send RawValue
    /// </summary>
    protected virtual CreateObject GetTypeCtor(EntityMapper mapper)
    {
        Type type = mapper.ForType;
        List<CreateObject> Mappings = new List<CreateObject>();
        bool returnZeroParamNull = false;
        foreach (ConstructorInfo ctor in type.GetConstructors())
        {
            ParameterInfo[] pars = ctor.GetParameters();
            // For 0 parameters, we can let the Reflection.CreateInstance handle it, unless they've specified a [BsonCtor] attribute on a different constructor.
            if (pars.Length == 0)
            {
                returnZeroParamNull = true;
                continue;
            }

            KeyValuePair<string, Type>[] paramMap = new KeyValuePair<string, Type>[pars.Length];
            int i;
            for (i = 0; i < pars.Length; i++)
            {
                ParameterInfo par = pars[i];
                MemberMapper mi = null;
                foreach (MemberMapper member in mapper.Members)
                {
                    if (member.MemberName.ToLower() == par.Name.ToLower() && member.DataType == par.ParameterType)
                    {
                        mi = member;
                        break;
                    }
                }

                if (mi == null)
                {
                    break;
                }

                paramMap[i] = new KeyValuePair<string, Type>(mi.FieldName, mi.DataType);
            }

            if (i < pars.Length)
            {
                continue;
            }

            CreateObject toAdd = (BsonDocument value) =>
                Activator.CreateInstance(type, paramMap.Select(x =>
                    this.Deserialize(x.Value, value[x.Key])).ToArray());
            if (ctor.GetCustomAttribute<CollectionConstructorAttribute>() != null)
            {
                return toAdd;
            }
            else
            {
                Mappings.Add(toAdd);
            }
        }

        if (returnZeroParamNull)
        {
            return null;
        }

        return Mappings.FirstOrDefault();
    }
}