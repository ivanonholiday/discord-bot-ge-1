using MainApp.Jobs;
using Quartz;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((ctx, services) =>
    {
        services.AddQuartz();
        services.AddQuartzHostedService(p => p.WaitForJobsToComplete = true);
    })
    .Build();

var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
var isDevelopment = !string.IsNullOrWhiteSpace(envName) && envName == "Development";

var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

// https://www.freeformatter.com/cron-expression-generator-quartz.html

const string cronEveryHour = "0 0 0/1 ? * * *"; // everyHour starting at 00:00
const string cronEvery2200 = "0 0 14 ? * * *"; // every 10:00pm HKT
const string cronEvery2225 = "0 25 14 ? * * *"; // every 10:25pm HKT

var jobTypes = new Dictionary<Type, string>
{
    [typeof(FetchCurrentIpJob)] = cronEveryHour,
    [typeof(NotifyBuffJob1)] = cronEvery2200,
    [typeof(NotifyBuffJob2)] = cronEvery2225,
};

var i = 1;
foreach (var (type, cronExpression) in jobTypes)
{
    const string name = nameof(type);
    var jobName = $"job_{i}_{name}";
    var triggerName = $"trigger_{i}_{name}";
    var groupName = $"group_{i}_{name}";

    var job = JobBuilder.Create(type).WithIdentity(jobName, groupName).Build();

    var trigger = isDevelopment
        ? TriggerBuilder.Create().WithIdentity(triggerName, groupName).StartNow().Build()
        : TriggerBuilder.Create().WithIdentity(triggerName, groupName).WithCronSchedule(cronExpression).Build();

    await scheduler.ScheduleJob(job, trigger);
    i++;
}

await scheduler.Start();

// will block until the last running job completes
await builder.RunAsync();