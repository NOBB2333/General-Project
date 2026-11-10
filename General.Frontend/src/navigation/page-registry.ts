import { routeNames } from '../router/route.types.ts';

export interface PageRegistration {
  backendComponent: string;
  backendName: string;
  backendPath: string;
  defaultHome?: boolean;
  homeAliases?: readonly string[];
  requiredAccessCodes: readonly string[];
  routeName: string;
  routePath: string;
}

export interface MenuCatalogRegistration {
  backendComponent: string;
  backendName: string;
  backendPath: string;
}

export interface ResolvedMenuNode {
  affixTab: boolean;
  children: ResolvedMenuNode[];
  defaultHome: boolean;
  homeAliases?: readonly string[];
  icon: null | string;
  kind: 'group' | 'page';
  label: string;
  localPath: null | string;
  routeName: null | string;
}

export const pageRegistry: readonly PageRegistration[] = [
  {
    backendComponent: '/platform/cache/index',
    backendName: 'PlatformCache',
    backendPath: '/platform/cache',
    requiredAccessCodes: ['Platform.Cache.Manage'],
    routeName: routeNames.platformCache,
    routePath: '/platform/cache',
  },
  {
    backendComponent: '/platform/scheduler/index',
    backendName: 'PlatformScheduler',
    backendPath: '/platform/scheduler',
    requiredAccessCodes: ['Platform.Scheduler.View'],
    routeName: routeNames.platformScheduler,
    routePath: '/platform/scheduler',
  },
  {
    backendComponent: '/platform/system-monitor/index',
    backendName: 'PlatformSystemMonitor',
    backendPath: '/platform/system-monitor',
    requiredAccessCodes: ['Platform.SystemMonitor.View'],
    routeName: routeNames.platformSystemMonitor,
    routePath: '/platform/system-monitor',
  },
  {
    backendComponent: '/platform/configs/index',
    backendName: 'PlatformConfigs',
    backendPath: '/platform/configs',
    requiredAccessCodes: ['Platform.Config.Manage'],
    routeName: routeNames.platformConfigs,
    routePath: '/platform/configs',
  },
  {
    backendComponent: '/platform/audit-logs/index',
    backendName: 'PlatformAuditLogs',
    backendPath: '/platform/audit-logs',
    requiredAccessCodes: ['Platform.AuditLog.View'],
    routeName: routeNames.platformAuditLogs,
    routePath: '/platform/audit-logs',
  },
  {
    backendComponent: '/platform/workspace/index',
    backendName: 'PlatformWorkspace',
    backendPath: '/platform/workspace',
    defaultHome: true,
    requiredAccessCodes: [],
    routeName: routeNames.platformWorkspace,
    routePath: '/platform/workspace',
  },
  {
    backendComponent: '/platform/settings/index',
    backendName: 'PlatformSettings',
    backendPath: '/platform/settings',
    requiredAccessCodes: [],
    routeName: routeNames.settings,
    routePath: '/settings',
  },
  {
    backendComponent: '/platform/users/index',
    backendName: 'PlatformUsers',
    backendPath: '/platform/users',
    requiredAccessCodes: [],
    routeName: routeNames.platformUsers,
    routePath: '/platform/users',
  },
  {
    backendComponent: '/platform/organization/index',
    backendName: 'PlatformOrganization',
    backendPath: '/platform/organization',
    requiredAccessCodes: [],
    routeName: routeNames.platformOrganization,
    routePath: '/platform/organization',
  },
  {
    backendComponent: '/platform/roles/index',
    backendName: 'PlatformRoles',
    backendPath: '/platform/roles',
    requiredAccessCodes: ['Platform.Role.Manage'],
    routeName: routeNames.platformRoles,
    routePath: '/platform/roles',
  },
  {
    backendComponent: '/platform/menus/index',
    backendName: 'PlatformMenus',
    backendPath: '/platform/menus',
    requiredAccessCodes: ['Platform.Menu.Manage'],
    routeName: routeNames.platformMenus,
    routePath: '/platform/menus',
  },
  {
    backendComponent: '/platform/dicts/index',
    backendName: 'PlatformDicts',
    backendPath: '/platform/dicts',
    requiredAccessCodes: ['Platform.Dict.Manage'],
    routeName: routeNames.platformDicts,
    routePath: '/platform/dicts',
  },
  {
    backendComponent: '/platform/open-api/index',
    backendName: 'PlatformOpenApi',
    backendPath: '/platform/open-api',
    requiredAccessCodes: ['Platform.OpenApi.Manage'],
    routeName: routeNames.platformOpenApi,
    routePath: '/platform/open-api',
  },
  {
    backendComponent: '/platform/files/index',
    backendName: 'PlatformFiles',
    backendPath: '/platform/files',
    requiredAccessCodes: ['Platform.File.Manage'],
    routeName: routeNames.platformFiles,
    routePath: '/platform/files',
  },
  {
    backendComponent: '/platform/recycle-bin/index',
    backendName: 'PlatformRecycleBin',
    backendPath: '/platform/recycle-bin',
    requiredAccessCodes: ['Platform.RecycleBin.Manage'],
    routeName: routeNames.platformRecycleBin,
    routePath: '/platform/recycle-bin',
  },
  {
    backendComponent: '/platform/update-logs/index',
    backendName: 'PlatformUpdateLogs',
    backendPath: '/platform/update-logs',
    requiredAccessCodes: [],
    routeName: routeNames.platformUpdateLogs,
    routePath: '/platform/update-logs',
  },
  {
    backendComponent: '/platform/tenants/index',
    backendName: 'PlatformTenants',
    backendPath: '/platform/tenants',
    requiredAccessCodes: ['Platform.Tenant.View'],
    routeName: routeNames.platformTenants,
    routePath: '/platform/tenants',
  },
  {
    backendComponent: '/integration/connectors/index',
    backendName: 'IntegrationConnectors',
    backendPath: '/integration/connectors',
    requiredAccessCodes: ['Integration.Connector.View'],
    routeName: routeNames.integrationConnectors,
    routePath: '/integration/connectors',
  },
  {
    backendComponent: '/integration/bindings/index',
    backendName: 'IntegrationBindings',
    backendPath: '/integration/bindings',
    requiredAccessCodes: ['Integration.Binding.View'],
    routeName: routeNames.integrationBindings,
    routePath: '/integration/bindings',
  },
  {
    backendComponent: '/integration/groups/index',
    backendName: 'IntegrationGroups',
    backendPath: '/integration/groups',
    requiredAccessCodes: ['Integration.Binding.View'],
    routeName: routeNames.integrationGroups,
    routePath: '/integration/groups',
  },
  {
    backendComponent: '/integration/templates/index',
    backendName: 'IntegrationTemplates',
    backendPath: '/integration/templates',
    requiredAccessCodes: ['Integration.Template.View'],
    routeName: routeNames.integrationTemplates,
    routePath: '/integration/templates',
  },
  {
    backendComponent: '/integration/rules/index',
    backendName: 'IntegrationRules',
    backendPath: '/integration/rules',
    requiredAccessCodes: ['Integration.Rule.View'],
    routeName: routeNames.integrationRules,
    routePath: '/integration/rules',
  },
  {
    backendComponent: '/integration/delivery-records/index',
    backendName: 'IntegrationDeliveryRecords',
    backendPath: '/integration/delivery-records',
    requiredAccessCodes: ['Integration.Delivery.View'],
    routeName: routeNames.integrationDeliveryRecords,
    routePath: '/integration/delivery-records',
  },
  {
    backendComponent: '/integration/sync-jobs/index',
    backendName: 'IntegrationSyncJobs',
    backendPath: '/integration/sync-jobs',
    requiredAccessCodes: ['Integration.Sync.View'],
    routeName: routeNames.integrationSyncJobs,
    routePath: '/integration/sync-jobs',
  },
  {
    backendComponent: '/integration/webhook-events/index',
    backendName: 'IntegrationWebhookEvents',
    backendPath: '/integration/webhook-events',
    requiredAccessCodes: ['Integration.Webhook.View'],
    routeName: routeNames.integrationWebhookEvents,
    routePath: '/integration/webhook-events',
  },
  {
    backendComponent: '/ai/assistants/index',
    backendName: 'AiAssistants',
    backendPath: '/ai/assistants',
    requiredAccessCodes: ['Ai.Assistant.Manage'],
    routeName: routeNames.aiAssistants,
    routePath: '/ai/assistants',
  },
  {
    backendComponent: '/ai/prompts/index',
    backendName: 'AiPrompts',
    backendPath: '/ai/prompts',
    requiredAccessCodes: ['Ai.Prompt.Manage'],
    routeName: routeNames.aiPrompts,
    routePath: '/ai/prompts',
  },
  {
    backendComponent: '/ai/agent-design/index',
    backendName: 'AiAgentDesign',
    backendPath: '/ai/agent-design',
    requiredAccessCodes: ['Ai.Agent.Manage'],
    routeName: routeNames.aiAgentDesign,
    routePath: '/ai/agent-design',
  },
  {
    backendComponent: '/ai/artifacts/index',
    backendName: 'AiArtifacts',
    backendPath: '/ai/artifacts',
    requiredAccessCodes: ['Ai.Artifact.Manage'],
    routeName: routeNames.aiArtifacts,
    routePath: '/ai/artifacts',
  },
  {
    backendComponent: '/ai/providers/index',
    backendName: 'AiProviders',
    backendPath: '/ai/providers',
    requiredAccessCodes: ['Ai.Provider.Manage'],
    routeName: routeNames.aiProviders,
    routePath: '/ai/providers',
  },
  {
    backendComponent: '/ai/conversations/index',
    backendName: 'AiConversations',
    backendPath: '/ai/conversations',
    requiredAccessCodes: ['Ai.Conversation.Use'],
    routeName: routeNames.aiConversations,
    routePath: '/ai/conversations',
  },
  {
    backendComponent: '/ai/conversation-management/index',
    backendName: 'AiConversationManagement',
    backendPath: '/ai/conversation-management',
    requiredAccessCodes: ['Ai.Conversation.Use'],
    routeName: routeNames.aiConversationManagement,
    routePath: '/ai/conversation-management',
  },
  {
    backendComponent: '/ai/web-search/index',
    backendName: 'AiWebSearch',
    backendPath: '/ai/web-search',
    requiredAccessCodes: ['Ai.WebSearch.Manage'],
    routeName: routeNames.aiWebSearch,
    routePath: '/ai/web-search',
  },
  {
    backendComponent: '/ai/mcp/index',
    backendName: 'AiMcp',
    backendPath: '/ai/mcp',
    requiredAccessCodes: ['Ai.Mcp.Manage'],
    routeName: routeNames.aiMcp,
    routePath: '/ai/mcp',
  },
  {
    backendComponent: '/ai/memory/index',
    backendName: 'AiMemory',
    backendPath: '/ai/memory',
    requiredAccessCodes: ['Ai.Memory.Manage'],
    routeName: routeNames.aiMemory,
    routePath: '/ai/memory',
  },
  {
    backendComponent: '/ai/jobs/index',
    backendName: 'AiJobs',
    backendPath: '/ai/jobs',
    requiredAccessCodes: ['Ai.Job.Manage'],
    routeName: routeNames.aiJobs,
    routePath: '/ai/jobs',
  },
  {
    backendComponent: '/ai/usage/index',
    backendName: 'AiUsage',
    backendPath: '/ai/usage',
    requiredAccessCodes: ['Ai.Usage.View'],
    routeName: routeNames.aiUsage,
    routePath: '/ai/usage',
  },
  {
    backendComponent: '/ai/writebacks/index',
    backendName: 'AiWritebacks',
    backendPath: '/ai/writebacks',
    requiredAccessCodes: ['Ai.Writeback.View'],
    routeName: routeNames.aiWritebacks,
    routePath: '/ai/writebacks',
  },
  {
    backendComponent: '/business/overview/index',
    backendName: 'BusinessOverview',
    backendPath: '/business/overview',
    requiredAccessCodes: ['Business.Report.View'],
    routeName: routeNames.businessOverview,
    routePath: '/business/overview',
  },
  {
    backendComponent: '/business/projects/index',
    backendName: 'BusinessProjects',
    backendPath: '/business/projects',
    requiredAccessCodes: [],
    routeName: routeNames.businessProjects,
    routePath: '/business/projects',
  },
  {
    backendComponent: '/business/reports/index',
    backendName: 'BusinessReports',
    backendPath: '/business/reports',
    requiredAccessCodes: ['Business.Report.View'],
    routeName: routeNames.businessReports,
    routePath: '/business/reports',
  },
  {
    backendComponent: '/project/workspace/index',
    backendName: 'ProjectWorkspace',
    backendPath: '/project/workspace',
    homeAliases: ['/project/pm-dashboard', '/project/pmo-overview'],
    requiredAccessCodes: [],
    routeName: 'page.projects-new.project-board',
    routePath: '/projects-new/project-board',
  },
  {
    backendComponent: '/project/projects/index',
    backendName: 'ProjectList',
    backendPath: '/project/projects',
    homeAliases: ['/project/projects'],
    requiredAccessCodes: [],
    routeName: 'page.projects-new.project-list',
    routePath: '/projects-new/project-list',
  },
  {
    backendComponent: '/project/detail/index',
    backendName: 'ProjectDetail',
    backendPath: '/project/detail',
    requiredAccessCodes: [],
    routeName: 'page.projects-new.project-list',
    routePath: '/projects-new/project-list',
  },
  {
    backendComponent: '/project/plan-templates/index',
    backendName: 'ProjectPlanTemplates',
    backendPath: '/project/plan-templates',
    requiredAccessCodes: ['Project.PlanTemplate.View'],
    routeName: 'page.projects.project-config',
    routePath: '/projects/project-config',
  },
  {
    backendComponent: '/project/tasks/index',
    backendName: 'ProjectTasks',
    backendPath: '/project/tasks',
    requiredAccessCodes: [],
    routeName: 'page.projects-new.my-tasks',
    routePath: '/projects-new/my-tasks',
  },
  {
    backendComponent: '/project/my-related/index',
    backendName: 'ProjectMyRelated',
    backendPath: '/project/my-related',
    homeAliases: ['/project/my-related'],
    requiredAccessCodes: [],
    routeName: 'page.projects-new.my-projects',
    routePath: '/projects-new/my-projects',
  },
];

export const menuCatalogRegistry: readonly MenuCatalogRegistration[] = [
  {
    backendComponent: 'BasicLayout',
    backendName: 'PlatformCenter',
    backendPath: '/platform',
  },
  {
    backendComponent: 'BasicLayout',
    backendName: 'IntegrationCenter',
    backendPath: '/integration',
  },
  {
    backendComponent: 'BasicLayout',
    backendName: 'ProjectCenter',
    backendPath: '/project',
  },
  {
    backendComponent: 'BasicLayout',
    backendName: 'BusinessCenter',
    backendPath: '/business',
  },
  {
    backendComponent: 'BasicLayout',
    backendName: 'AiCenter',
    backendPath: '/ai',
  },
];

export function hasRequiredAccessCodes(grantedCodes: readonly string[], requiredCodes: readonly string[]): boolean {
  const normalized = new Set(grantedCodes.map((code) => code.trim().toLowerCase()).filter(Boolean));
  return requiredCodes.every((code) => normalized.has(code.trim().toLowerCase()));
}

export function findPageRegistrationByRouteName(routeName: string): PageRegistration | undefined {
  return pageRegistry.find((page) => page.routeName === routeName);
}

function isSafeInternalPath(path: string): boolean {
  return (
    path.startsWith('/') &&
    !path.startsWith('//') &&
    !path.includes('\\') &&
    !path.includes('?') &&
    !path.includes('#') &&
    // oxlint-disable-next-line no-control-regex -- reject ASCII control characters in backend menu paths.
    !/[\u0000-\u001f]/u.test(path)
  );
}

function resolveNode(node: MenuApiRoute, grantedCodes: readonly string[], isRoot: boolean): ResolvedMenuNode | null {
  const meta = node.meta ?? { title: '' };
  if (
    typeof node.name !== 'string' ||
    typeof node.path !== 'string' ||
    !isSafeInternalPath(node.path) ||
    meta.link?.trim() ||
    node.redirect?.trim()
  ) {
    return null;
  }

  const page = pageRegistry.find(
    (entry) =>
      entry.backendName === node.name && entry.backendPath === node.path && entry.backendComponent === node.component,
  );
  if (page) {
    if (!hasRequiredAccessCodes(grantedCodes, page.requiredAccessCodes)) return null;
    return {
      affixTab: Boolean(meta.affixTab),
      children: [],
      defaultHome: Boolean(page.defaultHome),
      homeAliases: page.homeAliases ?? [],
      icon: meta.icon || null,
      kind: 'page',
      label: meta.title?.trim() || page.backendName,
      localPath: page.routePath,
      routeName: page.routeName,
    };
  }

  const catalog = menuCatalogRegistry.find(
    (entry) =>
      entry.backendName === node.name && entry.backendPath === node.path && entry.backendComponent === node.component,
  );
  if (!isRoot || !catalog) return null;

  const children = Array.isArray(node.children)
    ? node.children.flatMap((child) => {
        const resolved = resolveNode(child, grantedCodes, false);
        return resolved ? [resolved] : [];
      })
    : [];
  if (children.length === 0) return null;

  return {
    affixTab: false,
    children,
    defaultHome: false,
    homeAliases: [],
    icon: meta.icon || null,
    kind: 'group',
    label: meta.title?.trim() || catalog.backendName,
    localPath: null,
    routeName: null,
  };
}

type MenuApiRoute = {
  children?: MenuApiRoute[] | null;
  component?: null | string;
  meta?: {
    affixTab?: boolean | null;
    icon?: null | string;
    link?: null | string;
    title?: string | null;
  } | null;
  name?: string;
  path?: string;
  redirect?: null | string;
};

export function adaptBackendMenuTree(
  routes: readonly MenuApiRoute[],
  grantedCodes: readonly string[],
): ResolvedMenuNode[] {
  return routes.flatMap((route) => {
    const resolved = resolveNode(route, grantedCodes, true);
    return resolved ? [resolved] : [];
  });
}

export function getResolvedPages(nodes: readonly ResolvedMenuNode[]): ResolvedMenuNode[] {
  return nodes.flatMap((node) => (node.kind === 'page' ? [node] : getResolvedPages(node.children)));
}

export function resolveAuthorizedHomePath(requestedPath: string, pages: readonly ResolvedMenuNode[]): null | string {
  const safeRequest = isSafeInternalPath(requestedPath) ? requestedPath.replace(/\/+$/u, '') || '/' : '';
  const requested = pages.find((page) => page.localPath === safeRequest || page.homeAliases?.includes(safeRequest));
  if (requested?.localPath) return requested.localPath;

  const defaultPage = pages.find((page) => page.defaultHome) ?? pages.find((page) => page.affixTab);
  return defaultPage?.localPath ?? pages[0]?.localPath ?? null;
}
