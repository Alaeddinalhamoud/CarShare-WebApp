using Quartz;
using Quartz.Spi;
using System;

namespace CarShareV1.Jobs
{
    public class ServiceJobFactory : IJobFactory
    {
        readonly IServiceProvider _provider;
        public ServiceJobFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public  IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                return  _provider.GetService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception e)
            {
                throw new SchedulerException(string.Format("Problem while instansting job {0} ", bundle.JobDetail.Key), e);
            }
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
