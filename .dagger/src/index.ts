/**
 * A generated module for QuickStart functions
 *
 * This module has been generated via dagger init and serves as a reference to
 * basic module structure as you get started with Dagger.
 *
 * Two functions have been pre-created. You can modify, delete, or add to them,
 * as needed. They demonstrate usage of arguments and return types using simple
 * echo and grep commands. The functions can be called from the dagger CLI or
 * from one of the SDKs.
 *
 * The first line in this comment block is a short description line and the
 * rest is a long description with more detail on the module's purpose or usage,
 * if appropriate. All modules should have a short description.
 */
import { dag, Container, Directory, Changeset, object, func, argument } from "@dagger.io/dagger"

@object()
export class NukeBuildOutput {
  artifacts: Directory
  results: Directory

  constructor(artifacts: Directory, results: Directory) {
    this.artifacts = artifacts;
    this.results = results;
  }

  // ✅ Add a function to export both directories
  @func()
  async export(hostPath: string): Promise<string> {
    // Export 'artifacts' to a subdirectory named 'artifacts' inside the hostPath
    await this.artifacts.export(`${hostPath}/artifacts`);

    // Export 'results' to a subdirectory named 'results' inside the hostPath
    await this.results.export(`${hostPath}/results`);

    // Return a message indicating success (or void, Dagger usually handles the success check)
    return `Successfully exported artifacts and results to ${hostPath}`;
  }
}

@object()
export class DaggerBuild {

  @func()
  async build(
    @argument({ defaultPath: "." }) source: Directory,
    base?: Container,
    target: string = "All",
  ): Promise<Changeset> {

    const container = (base ?? dag.container().from("mcr.microsoft.com/dotnet/sdk:10.0").withWorkdir("/app"))
      .withDirectory(".", source)
    const before = container.directory(".")
    const after = container
      .withExec(["dotnet", "tool", "restore"])
      .withExec(["dotnet", "tool", "run", "nuke", "--target", target])
      .directory(".")
    return after.changes(before)
  }

  @func()
  async nukeBuild(
      directory: Directory,
      target: string = "All",
      image: string = "mcr.microsoft.com/dotnet/sdk:10.0"): Promise<NukeBuildOutput> {
    const container = dag
      .container()
      .from(image)
      .withMountedDirectory("/app", directory)
      .withWorkdir("/app")
      // .withExec(["pwd"])
      // .withExec(["ls", "-l"]) // ensure .config/dotnet-tools.json is actually there
      // .withExec(["cat", ".config/dotnet-tools.json"])
      .withExec(["dotnet", "tool", "restore"])
      .withExec(["dotnet", "tool", "run", "nuke", "--target", target])
      ;

    const results: Directory = container.directory("./results");
    const artifacts: Directory = container.directory("./artifacts");

    await results.entries();
    await artifacts.entries();

    // await artifacts.export("./artifacts");

    // // await results.export("./results");
    // await artifacts.export("./artifacts", { wipe: true });

    return new NukeBuildOutput(artifacts, results);
  }
}
