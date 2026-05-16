using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ClockworkSurvivor.EditorTools
{
    public static class CiBuild
    {
        private const string DefaultScenePath = "Assets/Scenes/ClockworkSurvivor.unity";
        private const string DefaultProductName = "ClockworkSurvivor";

        public static void Build()
        {
            ClockworkBootstrap.SetupProjectFromMenu();

            BuildTarget target = ResolveBuildTarget();
            string outputPath = ResolveBuildPath(target);
            string[] scenes = ResolveScenes();

            EnsureOutputDirectory(target, outputPath);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                locationPathName = outputPath,
                options = BuildOptions.None
            };

            Debug.Log($"Starting CI build. Target={target}, Output={outputPath}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Debug.Log($"CI build result: {summary.result}, Size={summary.totalSize} bytes, Time={summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"CI build failed with result {summary.result}");
            }
        }

        private static BuildTarget ResolveBuildTarget()
        {
            string targetName = Environment.GetEnvironmentVariable("BUILD_TARGET");
            if (string.IsNullOrWhiteSpace(targetName))
            {
                return BuildTarget.WebGL;
            }

            if (Enum.TryParse(targetName, true, out BuildTarget target))
            {
                return target;
            }

            throw new ArgumentException($"Unsupported BUILD_TARGET value: {targetName}");
        }

        private static string ResolveBuildPath(BuildTarget target)
        {
            string configuredPath = Environment.GetEnvironmentVariable("BUILD_PATH");
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return configuredPath.Replace('\\', '/');
            }

            switch (target)
            {
                case BuildTarget.WebGL:
                    return "Builds/WebGL";
                case BuildTarget.StandaloneLinux64:
                    return "Builds/Linux/" + DefaultProductName + ".x86_64";
                case BuildTarget.StandaloneWindows64:
                    return "Builds/Windows/" + DefaultProductName + ".exe";
                default:
                    return "Builds/" + target + "/" + DefaultProductName;
            }
        }

        private static string[] ResolveScenes()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .Where(File.Exists)
                .ToArray();

            if (scenes.Length > 0)
            {
                return scenes;
            }

            if (File.Exists(DefaultScenePath))
            {
                return new[] { DefaultScenePath };
            }

            throw new FileNotFoundException("No enabled build scenes were found.");
        }

        private static bool IsDirectoryOutput(BuildTarget target)
        {
            return target == BuildTarget.WebGL;
        }

        private static void EnsureOutputDirectory(BuildTarget target, string outputPath)
        {
            string directory = IsDirectoryOutput(target) ? outputPath : Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = ".";
            }

            Directory.CreateDirectory(directory);
        }
    }
}
