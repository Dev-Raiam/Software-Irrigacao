namespace Toolbox.Industrial.Core.Communication.Api
{
    public interface MediaTypes
    {
        public const string Default = "application/json";
        public const string NoContent = "application/vnd.data.no-content";

        public interface V1
        {
            public interface Aggregate
            {
                public const string Full = "application/vnd.data.aggregate.full.v1+json";
                public const string Compact = "application/vnd.data.aggregate.compact.v1+json";
                public const string Minimal = "application/vnd.data.aggregate.minimal.v1+json";
            }

            public interface Tree
            {
                public const string Full = "application/vnd.data.tree.full.v1+json";
                public const string Compact = "application/vnd.data.tree.compact.v1+json";
                public const string Minimal = "application/vnd.data.tree.minimal.v1+json";
            }

            public const string Full = "application/vnd.data.full.v1+json";
            public const string Compact = "application/vnd.data.compact.v1+json";
            public const string Minimal = "application/vnd.data.minimal.v1+json";
        }

        public interface V2
        {
            public const string Full = "application/vnd.data.full.v2+json";
            public const string Compact = "application/vnd.data.compact.v2+json";
            public const string Minimal = "application/vnd.data.minimal.v2+json";
        }

        public interface Integration
        {
            public const string V1 = "application/vnd.data.integration.v1+json";
        }

        public interface Industrial
        {
            public const string V1 = "application/vnd.data.industrial.v1+json";
        }
    }
}
