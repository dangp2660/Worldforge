using System;

namespace Worldforge.Core.Attributes
{
    /// <summary>
    /// Marks a class or interface as a target for in-game method testing.
    /// Used by the Dynamic Method Tester tool for categorization and display.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public sealed class TestTargetAttribute : Attribute
    {
        public string Category { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public TestTargetAttribute(string category = null, string displayName = null, int order = 0, string description = null)
        {
            Category = category;
            DisplayName = displayName;
            Order = order;
            Description = description;
        }
    }

    /// <summary>
    /// Marks a method for in-game testing.
    /// Allows designating whether this is a primary test method (highlighted/starred)
    /// and configuring custom display name, description, and order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class TestMethodAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsPrimary { get; set; }

        public int Order { get; set; }

        public string Category { get; set; }

        public TestMethodAttribute(
            string displayName = null,
            bool isPrimary = true,
            int order = 0,
            string description = null,
            string category = null)
        {
            DisplayName = displayName;
            IsPrimary = isPrimary;
            Order = order;
            Description = description;
            Category = category;
        }
    }

    /// <summary>
    /// Explicitly excludes a method from appearing in the Method Tester tool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class TestMethodIgnoreAttribute : Attribute
    {
    }
}
