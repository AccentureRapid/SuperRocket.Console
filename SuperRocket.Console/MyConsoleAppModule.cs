using System.Reflection;
using Abp.EntityFramework;
using Abp.Modules;
using System;
using System.Transactions;

namespace AbpEfConsoleApp
{
    //Defining a module depends on AbpEntityFrameworkModule
    [DependsOn(typeof(AbpEntityFrameworkModule))]
    public class MyConsoleAppModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsolationLevel = IsolationLevel.ReadCommitted;
            Configuration.UnitOfWork.Timeout = TimeSpan.FromMinutes(30);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}