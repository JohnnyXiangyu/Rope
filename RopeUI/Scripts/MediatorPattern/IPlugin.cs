namespace RopeUI.Scripts.MediatorPattern;
public interface IPlugin
{
    /// <summary>
    /// Called first, there's no guarantee your desired services are in here.
    /// </summary>
    void ConfigureServices(DependencyManger depdencyManager);

    /// <summary>
    /// At least all dependents have been configured once, it's safe to assume the service you want is here.
    /// </summary>
    void ContainerSetup(DependencyManger dependencyManger);
}
