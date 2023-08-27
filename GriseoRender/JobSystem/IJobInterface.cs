using System.Collections;

namespace GriseoRenderer.JobSystem;

public interface IYieldJob
{
    IEnumerator Execute();
}

public interface IJob
{
    void Execute();
}

public interface IJobFor
{
    void Execute(int index);
}

public interface IYieldJobFor
{
    IEnumerator Execute(int startIndex, int endIndex);
}