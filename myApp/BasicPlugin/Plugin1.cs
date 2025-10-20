
using Microsoft.Xrm.Sdk;
using System;

namespace BasicPlugin
{
    // You can keep the class name Plugin1, or rename it (the name just shows up in PRT)
    public class Plugin1 : PluginBase
    {
        public Plugin1(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(Plugin1))
        { }

        // Entry point for your business logic
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
                throw new ArgumentNullException(nameof(localPluginContext));

            var context = localPluginContext.PluginExecutionContext;
            var service = localPluginContext.PluginUserService;      // IOrganizationService
            var trace   = localPluginContext.TracingService;

            // Run only on Create of account (still register the step that way)
            if (!string.Equals(context.MessageName, "Create", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(context.PrimaryEntityName, "account", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Avoid accidental recursion
            if (context.Depth > 1) return;

            try
            {
                // Due date in 7 days (UTC)
                var due = DateTime.UtcNow.AddDays(7);

                var task = new Entity("task");
                task["subject"]        = "Send e-mail to the new customer.";
                task["description"]    = "Follow up with the customer. Check if there are any new issues that need resolution.";
                task["scheduledstart"] = due;
                task["scheduledend"]   = due;
                task["category"]       = context.PrimaryEntityName;

                // Regarding = the account that was created
                if (context.PrimaryEntityId != Guid.Empty)
                {
                    task["regardingobjectid"] = new EntityReference("account", context.PrimaryEntityId);
                }

                trace.Trace("FollowupPlugin: Creating the task activity.");
                service.Create(task);
            }
            catch (Exception ex)
            {
                trace.Trace("FollowupPlugin error: {0}", ex.ToString());
                throw; // surfaces to async system job if running async
            }
        }
    }
}

