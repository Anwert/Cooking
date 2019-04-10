using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace Cooking
{
	public class AutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			var assemblies = new[] {Assembly.GetAssembly(typeof(AutofacModule))};
			
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => t.Name.EndsWith("Repository")).AsSelf().SingleInstance();
			builder.RegisterAssemblyTypes(assemblies)
				.Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Helper")).AsSelf().SingleInstance();
		}
	}
}
