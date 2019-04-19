namespace Common
{
    public interface IWorker
    {
        void Start(string containerId);

        void Stop();
    }
}