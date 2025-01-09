﻿using System;
using System.Linq;
using System.Linq.Expressions;
using static LeoDB.Constants;

namespace LeoDB
{
    public partial class LeoCollection<T>
    {
        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        public ILeoCollection<T> Include<K>(Expression<Func<T, K>> keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            var path = _mapper.GetExpression(keySelector);

            return this.Include(path);
        }

        /// <summary>
        /// Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef documents
        /// Returns a new Collection with this action included
        /// </summary>
        public ILeoCollection<T> Include(BsonExpression keySelector)
        {
            if (string.IsNullOrEmpty(keySelector)) throw new ArgumentNullException(nameof(keySelector));

            // cloning this collection and adding this include
            var newcol = new LeoCollection<T>(_collection, _autoId, _engine, _mapper);

            newcol._includes.AddRange(_includes);
            newcol._includes.Add(keySelector);

            return newcol;
        }
    }
}