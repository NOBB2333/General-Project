using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260624193000_AddAiAgentRuntimeFoundation")]
    public partial class AddAiAgentRuntimeFoundation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppAiSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DefaultMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Instruction = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    InputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    OutputSchemaJson = table.Column<string>(type: "TEXT", nullable: false),
                    ToolCodesJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AppAiSkills", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AppAiPersonas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoleType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PersonalityPrompt = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    BoundaryPrompt = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    StylePrompt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AppAiPersonas", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AppAiCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SlashName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Mode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SkillCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PromptTemplateCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AgentCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InputPlaceholder = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExampleInput = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AppAiCommands", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AppAiArtifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtifactCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RendererType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    JsonSchema = table.Column<string>(type: "TEXT", nullable: false),
                    ExamplePayloadJson = table.Column<string>(type: "TEXT", nullable: false),
                    SystemHint = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AppAiArtifacts", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AppAiAgentDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RoleType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PersonaCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DefaultModelId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SystemPrompt = table.Column<string>(type: "character varying(16000)", maxLength: 16000, nullable: false),
                    ToolCodesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SkillCodesJson = table.Column<string>(type: "TEXT", nullable: false),
                    RuntimeConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltin = table.Column<bool>(type: "boolean", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAiAgentDefinitions", x => x.Id);
                    table.ForeignKey("FK_AppAiAgentDefinitions_AppAiModels_DefaultModelId", x => x.DefaultModelId, "AppAiModels", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AppAiWebSearchConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ApiKeyCipherText = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    AllowDomainsJson = table.Column<string>(type: "TEXT", nullable: false),
                    DenyDomainsJson = table.Column<string>(type: "TEXT", nullable: false),
                    MaxResults = table.Column<int>(type: "integer", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    ConfigJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Remark = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_AppAiWebSearchConfigs", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AppAiWebSearchRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfigId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Query = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ResultCount = table.Column<int>(type: "integer", nullable: false),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequestJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResponseSummaryJson = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAiWebSearchRecords", x => x.Id);
                    table.ForeignKey("FK_AppAiWebSearchRecords_AppAiWebSearchConfigs_ConfigId", x => x.ConfigId, "AppAiWebSearchConfigs", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_AppAiWebSearchRecords_AppAiConversations_ConversationId", x => x.ConversationId, "AppAiConversations", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_AppAiWebSearchRecords_AppAiMessages_MessageId", x => x.MessageId, "AppAiMessages", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex("IX_AppAiSkills_TenantId_SkillCode", "AppAiSkills", new[] { "TenantId", "SkillCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiSkills_Category_IsEnabled_Sort", "AppAiSkills", new[] { "Category", "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex("IX_AppAiPersonas_TenantId_PersonaCode", "AppAiPersonas", new[] { "TenantId", "PersonaCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiPersonas_RoleType_IsEnabled_Sort", "AppAiPersonas", new[] { "RoleType", "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex("IX_AppAiCommands_TenantId_CommandCode", "AppAiCommands", new[] { "TenantId", "CommandCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiCommands_TenantId_SlashName", "AppAiCommands", new[] { "TenantId", "SlashName" });
            migrationBuilder.CreateIndex("IX_AppAiCommands_IsEnabled_Sort", "AppAiCommands", new[] { "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex("IX_AppAiArtifacts_TenantId_ArtifactCode", "AppAiArtifacts", new[] { "TenantId", "ArtifactCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiArtifacts_Category_RendererType_IsEnabled_Sort", "AppAiArtifacts", new[] { "Category", "RendererType", "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex("IX_AppAiAgentDefinitions_DefaultModelId", "AppAiAgentDefinitions", "DefaultModelId");
            migrationBuilder.CreateIndex("IX_AppAiAgentDefinitions_TenantId_AgentCode", "AppAiAgentDefinitions", new[] { "TenantId", "AgentCode" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiAgentDefinitions_RoleType_IsEnabled_Sort", "AppAiAgentDefinitions", new[] { "RoleType", "IsEnabled", "Sort" });
            migrationBuilder.CreateIndex("IX_AppAiWebSearchConfigs_IsDefault", "AppAiWebSearchConfigs", "IsDefault");
            migrationBuilder.CreateIndex("IX_AppAiWebSearchConfigs_ProviderType_IsEnabled", "AppAiWebSearchConfigs", new[] { "ProviderType", "IsEnabled" });
            migrationBuilder.CreateIndex("IX_AppAiWebSearchConfigs_TenantId_Name", "AppAiWebSearchConfigs", new[] { "TenantId", "Name" }, unique: true);
            migrationBuilder.CreateIndex("IX_AppAiWebSearchRecords_ConfigId_CreationTime", "AppAiWebSearchRecords", new[] { "ConfigId", "CreationTime" });
            migrationBuilder.CreateIndex("IX_AppAiWebSearchRecords_ConversationId_CreationTime", "AppAiWebSearchRecords", new[] { "ConversationId", "CreationTime" });
            migrationBuilder.CreateIndex("IX_AppAiWebSearchRecords_MessageId", "AppAiWebSearchRecords", "MessageId");
            migrationBuilder.CreateIndex("IX_AppAiWebSearchRecords_TenantId_Status_CreationTime", "AppAiWebSearchRecords", new[] { "TenantId", "Status", "CreationTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AppAiWebSearchRecords");
            migrationBuilder.DropTable("AppAiAgentDefinitions");
            migrationBuilder.DropTable("AppAiArtifacts");
            migrationBuilder.DropTable("AppAiCommands");
            migrationBuilder.DropTable("AppAiPersonas");
            migrationBuilder.DropTable("AppAiSkills");
            migrationBuilder.DropTable("AppAiWebSearchConfigs");
        }
    }
}
