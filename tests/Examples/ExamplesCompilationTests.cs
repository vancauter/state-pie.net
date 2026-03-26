using System.Diagnostics;
using Xunit;

namespace Examples.Compilation.Tests
{
    public class ExamplesCompilationTests
    {
        [Fact]
        public void AllExampleRunProjects_WhenBuilt_Succeeds()
        {
            // Find repository root by locating AtomicState.sln
            var dir = Directory.GetCurrentDirectory();
            var info = new DirectoryInfo(dir);
            FileInfo? sln = null;
            while (info != null)
            {
                var candidate = Path.Combine(info.FullName, "AtomicState.sln");
                if (File.Exists(candidate)) { sln = new FileInfo(candidate); break; }
                info = info.Parent;
            }

            Assert.NotNull(sln);
            var repoRoot = sln!.Directory!.FullName;
            var examplesDir = Path.Combine(repoRoot, "dev", "examples");
            Assert.True(Directory.Exists(examplesDir), $"Examples dir not found: {examplesDir}");

            var projects = Directory.GetFiles(examplesDir, "run-*.csproj", SearchOption.TopDirectoryOnly);
            Assert.NotEmpty(projects);

            foreach (var proj in projects)
            {
                var psi = new ProcessStartInfo("dotnet", $"build \"{proj}\" --nologo")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = repoRoot
                };

                using var p = Process.Start(psi);
                Assert.NotNull(p);
                var outText = p!.StandardOutput.ReadToEnd();
                var errText = p.StandardError.ReadToEnd();
                p.WaitForExit();
                Assert.Equal(0, p.ExitCode);
            }
        }
    }
}
