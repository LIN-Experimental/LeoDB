﻿namespace LeoDB.Engine;

public partial class LeoEngine
{
    /// <summary>
    /// Recovery datafile using a rebuild process. Run only on "Open" database
    /// </summary>
    private void Recovery(Collation collation)
    {
        // run build service
        var rebuilder = new RebuildService(_settings);
        var options = new RebuildOptions
        {
            Collation = collation,
            Password = _settings.Password,
            IncludeErrorReport = true
        };

        // run rebuild process
        rebuilder.Rebuild(options);
    }
}