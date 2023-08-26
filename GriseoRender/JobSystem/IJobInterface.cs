namespace GriseoRenderer.JobSystem;

public interface IJob
{
    void Execute();
}

public interface IJobFor
{
    void Execute(int index);
}