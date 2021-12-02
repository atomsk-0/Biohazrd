﻿using ClangSharp.Pathogen;
using System;
using System.Diagnostics;

namespace Biohazrd.CSharp
{
    public sealed record CSharpGenerationOptions
    {
        public bool DumpClangInfo { get; init; } = false;
        public ClangSharpInfoDumper.Options DumpOptions { get; init; }

        public bool HideTrampolinesFromDebugger { get; init; } = true;

        private TargetRuntime _TargetRuntime = TargetRuntime.Default;
        private TargetLanguageVersion _TargetLangaugeVersion = TargetLanguageVersion.Default;

        /// <summary>The target runtime to generate code for.</summary>
        /// <remarks>
        /// Always returns a specific version, will not return <see cref="TargetRuntime.Default"/>
        ///
        /// If this is set to <see cref="TargetRuntime.Default"/>, will return the runtime corresponding to the value of <see cref="TargetLanguageVersion"/>
        /// or if it is also defaulted: <see cref="TargetRuntime.Net6"/>
        /// </remarks>
        public TargetRuntime TargetRuntime
        {
            get
            {
                if (_TargetRuntime is TargetRuntime.Default)
                {
                    Debug.Assert(Enum.IsDefined(_TargetLangaugeVersion));
                    return _TargetLangaugeVersion switch
                    {
                        TargetLanguageVersion.CSharp9 => TargetRuntime.Net5,
                        TargetLanguageVersion.CSharp10 => TargetRuntime.Net6,
                        _ => TargetRuntime.Net6
                    };
                }

                return _TargetRuntime;
            }

            init
            {
                if (!Enum.IsDefined(value))
                { throw new ArgumentOutOfRangeException($"The specified target runtime is invalid."); }

                _TargetRuntime = value;
            }
        }

        /// <summary>The target C# language version to generate code for.</summary>
        /// <remarks>
        /// Always returns a specific version, will not return <see cref="TargetLanguageVersion.Default"/>
        ///
        /// If this is set to <see cref="TargetLanguageVersion.Default"/>, will return the runtime corresponding to the value of <see cref="TargetRuntime"/>
        /// or if it is also defaulted: <see cref="TargetLanguageVersion.CSharp10"/>
        /// </remarks>
        public TargetLanguageVersion TargetLanguageVersion
        {
            get
            {
                if (_TargetLangaugeVersion is TargetLanguageVersion.Default)
                {
                    Debug.Assert(Enum.IsDefined(_TargetRuntime));
                    return _TargetRuntime switch
                    {
                        TargetRuntime.Net5 => TargetLanguageVersion.CSharp9,
                        TargetRuntime.Net6 => TargetLanguageVersion.CSharp10,
                        _ => TargetLanguageVersion.CSharp10
                    };
                }

                return _TargetLangaugeVersion;
            }

            init
            {
                if (!Enum.IsDefined(value))
                { throw new ArgumentOutOfRangeException($"The specified language version is invalid."); }

                _TargetLangaugeVersion = value;
            }
        }

        public CSharpGenerationOptions()
            => DumpOptions = ClangSharpInfoDumper.DefaultOptions;

        public static readonly CSharpGenerationOptions Default = new();
    }
}
