using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CloudTek.Build;

public enum SmartBuildError
{
    NoArtifacts = 0,
    MixedModules
}

public class SmartBuildException : Exception
{
    public SmartBuildError Error { get; init; }
    internal static IDictionary<SmartBuildError, string> messages = new Dictionary<SmartBuildError, string>()
    {
        { SmartBuildError.NoArtifacts, "The repository needs to define at least 1 artfact" },
        { SmartBuildError.MixedModules , "All repository artfacts must or must not specify the Module property. A mix is not allowed"}
    };
    
    public SmartBuildException(SmartBuildError error) : base(messages[error])
    {
    }
}