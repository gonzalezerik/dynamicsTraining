using Microsoft.Xrm.Sdk;
using System;

namespace ContactSyncPlugin
{
    public class ContactEmailSync : PluginBase
    {
        public ContactEmailSync(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ContactEmailSync))
        { }

        protected override void ExecuteDataversePlugin(ILocalPluginContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var context = ctx.PluginExecutionContext;
            var service = ctx.PluginUserService;
            var trace   = ctx.TracingService;

            // Only on Create of account
            if (!string.Equals(context.MessageName, "Create", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(context.PrimaryEntityName, "account", StringComparison.OrdinalIgnoreCase))
                return;

            // Avoid accidental recursion
            if (context.Depth > 1) return;

            // Build follow-up Task due in 7 days (UTC)
            var due = DateTime.UtcNow.AddDays(7);

            var task = new Entity("task");
            task["subject"]        = "Send e-mail to the new customer.";
            task["description"]    = "Follow up with the customer. Check if there are any new issues that need resolution.";
            task["scheduledstart"] = due;
            task["scheduledend"]   = due;
            task["category"]       = context.PrimaryEntityName;

            // Regarding = the created account
            if (context.PrimaryEntityId != Guid.Empty)
            {
                task["regardingobjectid"] = new EntityReference("account", context.PrimaryEntityId);
            }

            trace.Trace("FollowupPlugin: Creating the task activity.");
            service.Create(task);
        }
    }
}

