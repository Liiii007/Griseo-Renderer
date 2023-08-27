using System.Collections;

namespace GriseoRenderer.JobSystem;

public class Coroutine
{
    private JobHandle _dependency;
    private JobHandle _selfHandle;
    private IEnumerator _routine;

    public Coroutine(IEnumerator routine, JobHandle selfHandle, JobHandle dependency)
    {
        _routine = routine;
        _selfHandle = selfHandle;
        _dependency = dependency;
    }

    public bool MoveNext()
    {
        if (IsBlocking())
        {
            return true;
        }
        else
        {
            var result = _routine.MoveNext();
            
            //Add complete count when IEnumator end
            if (!result)
            {
                _selfHandle.CompleteOne();
            }

            return result;
        }
    }

    public bool IsBlocking()
    {
        if (_dependency != null && !_dependency.IsComplete)
        {
            return true;
        }

        if (_routine.Current is WaitForSubJob wait)
        {
            return wait.MoveNext();
        }

        return false;
    }
}