using System.Collections;

namespace GriseoRenderer.JobSystem;

public class JobBundle<T> : IEnumerator where T : struct, IYieldJobFor
{
    private int _minIndex;
    private int _maxIndex;
    private JobHandle _handle;
    private JobHandle _dependency;

    private T _job;

    public JobBundle(T job, int minIndex, int maxIndex, JobHandle handle, JobHandle dependency)
    {
        _handle = handle;
        _dependency = dependency;
        _job = job;
        _minIndex = minIndex;
        _maxIndex = maxIndex;
    }

    public IEnumerator Execute()
    {
        if (_dependency != null)
        {
            yield return new WaitForSubJob(_dependency);
        }

        yield return _job.Execute(_minIndex, _maxIndex);

        _handle.CompleteOne();
    }

    public bool MoveNext()
    {
        var ie = Execute();
        return ie.MoveNext();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public object Current { get; }
}