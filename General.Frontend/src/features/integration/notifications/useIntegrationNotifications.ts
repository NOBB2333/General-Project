import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { PageAction } from '@/components/page/page.contracts';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, type UiSelectOption } from '@/components/ui';
import { apiClient } from '@/api/request';
import { createIntegrationConnectorsApi } from '@/api/core/integration-connectors';
import {
  createIntegrationNotificationsApi,
  type IntegrationNotificationsApi,
} from '@/api/core/integration-notifications';
import { canAccess } from '@/directives/permission';
import { routeNames } from '@/router/route.types';
import {
  createRuleDraft,
  createTemplateDraft,
  ruleStatusLabel,
  templateStatusLabel,
  toRuleDraft,
  toRuleRow,
  toRuleSaveInput,
  toTemplateDraft,
  toTemplateRow,
  validateRuleDraft,
  validateTemplateDraft,
  type NotificationTab,
  type RuleDraft,
  type TemplateDraft,
} from './notifications.model';

type DialogKind = 'evaluation' | 'rule' | 'template' | null;

interface Props {
  initialTab?: NotificationTab;
}

const api = createIntegrationNotificationsApi(apiClient);
const connectorsApi = createIntegrationConnectorsApi(apiClient);
const pageSize = 20;
const templateStatusOptions = ['全部状态', '草稿', '启用', '停用'];
const templateTypeOptions = ['全部类型', '文本', 'Markdown', '卡片'];
const ruleStatusOptions = ['全部状态', '启用', '停用', '暂停'];
const statusValues: Record<string, string> = {
  草稿: 'Draft',
  启用: 'Active',
  停用: 'Disabled',
  暂停: 'Paused',
};
const templateTypeValues: Record<string, string> = {
  文本: 'Text',
  Markdown: 'Markdown',
  卡片: 'Card',
};

export function useIntegrationNotifications(props: Props) {
  const route = useRoute();
  const router = useRouter();
  const activeTab = computed<NotificationTab>(() => {
    if (route.name === routeNames.integrationRules) return 'rules';
    if (route.name === routeNames.integrationTemplates) return 'templates';
    return props.initialTab ?? 'templates';
  });
  const templates = ref<IntegrationNotificationsApi.Template[]>([]);
  const rules = ref<IntegrationNotificationsApi.Rule[]>([]);
  const totalCount = ref(0);
  const currentPage = ref(1);
  const keyword = ref('');
  const templateStatusFilter = ref('全部状态');
  const templateTypeFilter = ref('全部类型');
  const ruleStatusFilter = ref('全部状态');
  const loading = ref(false);
  const saving = ref(false);
  const previewLoading = ref(false);
  const evaluationLoading = ref(false);
  const loadError = ref('');
  const dialogError = ref('');
  const optionError = ref('');
  const activeRequest = ref<AbortController>();
  const searchTimer = ref<number>();
  const dialogKind = ref<DialogKind>(null);
  const dialogOpen = ref(false);
  const dialogReadOnly = ref(false);
  const templateDraft = reactive<TemplateDraft>(createTemplateDraft());
  const ruleDraft = reactive<RuleDraft>(createRuleDraft());
  const previewResult = ref<IntegrationNotificationsApi.TemplateRenderResult | null>(null);
  const evaluationEventCode = ref('');
  const evaluationVariablesJson = ref('{}');
  const evaluationResult = ref<IntegrationNotificationsApi.RuleEvaluationResult | null>(null);
  const templateOptions = ref<UiSelectOption[]>([]);
  const connectorOptions = ref<UiSelectOption[]>([]);
  const toggleTarget = ref<IntegrationNotificationsApi.Rule | null>(null);
  const toggleDialogOpen = ref(false);

  const canManageTemplate = computed(() => canAccess('Integration.Template.Manage'));
  const canTestTemplate = computed(() => canAccess('Integration.Template.Test'));
  const canManageRule = computed(() => canAccess('Integration.Rule.Manage'));
  const canViewTemplate = computed(() => canAccess('Integration.Template.View'));
  const canViewConnector = computed(() => canAccess('Integration.Connector.View'));
  const rows = computed<ListPageRow[]>(() =>
    activeTab.value === 'templates' ? templates.value.map(toTemplateRow) : rules.value.map(toRuleRow),
  );
  const pageLabel = computed(() => (activeTab.value === 'templates' ? '消息模板' : '通知规则'));
  const pageActions = computed<PageAction[]>(() => {
    const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
    if (activeTab.value === 'templates') {
      if (canManageTemplate.value) {
        actions.push({ id: 'create', kind: 'create', label: '新建模板', tone: 'primary' });
        actions.push({ id: 'edit', kind: 'custom', label: '编辑所选', requiresSelection: true });
      }
      if (canTestTemplate.value)
        actions.push({ id: 'preview', kind: 'custom', label: '预览所选', requiresSelection: true });
    } else {
      if (canManageRule.value) {
        actions.push({ id: 'create', kind: 'create', label: '新建规则', tone: 'primary' });
        actions.push({ id: 'edit', kind: 'custom', label: '编辑所选', requiresSelection: true });
        actions.push({ id: 'toggle', kind: 'custom', label: '启用 / 停用', requiresSelection: true });
        actions.push({ id: 'evaluate', kind: 'custom', label: '只评估事件' });
      }
    }
    return actions;
  });
  const pageModel = computed<ListPageModel>(() => {
    const isTemplateTab = activeTab.value === 'templates';
    return {
      actions: pageActions.value,
      columns: isTemplateTab
        ? [
            { key: 'name', title: '模板名称', width: 220, sticky: true },
            { key: 'code', title: '模板编码', width: 190 },
            { key: 'messageType', title: '消息类型', width: 130 },
            { key: 'status', title: '状态', width: 110, presentation: 'status' },
            { key: 'remark', title: '说明', width: 280 },
          ]
        : [
            { key: 'name', title: '规则名称', width: 190, sticky: true },
            { key: 'event', title: '事件编码 / 名称', width: 260 },
            { key: 'template', title: '消息模板', width: 190 },
            { key: 'channel', title: '消息渠道', width: 130 },
            { key: 'status', title: '状态', width: 100, presentation: 'status' },
            { key: 'lastTriggered', title: '最近触发', width: 190 },
          ],
      demoPath: route.path,
      description: isTemplateTab
        ? '维护消息模板、变量说明与预览。预览只渲染内容，不会发送消息。'
        : '按真实事件编码配置通知规则、接收人、静默时段和频控；此页不提供消息发送操作。',
      emptyHint: loading.value
        ? `正在读取${pageLabel.value}…`
        : loadError.value
          ? `${pageLabel.value}读取失败，请刷新重试。`
          : `当前筛选条件下没有${pageLabel.value}。`,
      filters: isTemplateTab
        ? [
            { key: 'status', label: '模板状态', options: templateStatusOptions },
            { key: 'type', label: '消息类型', options: templateTypeOptions },
          ]
        : [{ key: 'status', label: '规则状态', options: ruleStatusOptions }],
      key: `外部集成|${pageLabel.value}`,
      label: pageLabel.value,
      listScopes: [{ id: 'all', label: isTemplateTab ? '消息模板' : '通知规则', count: totalCount.value }],
      listTabs: [{ id: 'all', label: '全部记录' }],
      listViews: [{ id: 'list', label: '列表' }],
      module: '外部集成',
      moduleSlug: 'integration',
      pageSlug: isTemplateTab ? 'integration-templates' : 'integration-rules',
      pageSize,
      panels: [],
      path: route.path,
      primaryAction: pageActions.value.find((action) => action.kind === 'create')?.label ?? '刷新',
      replication: { status: 'implemented', referenceUrl: route.path },
      routeName: String(route.name ?? 'integration-notifications'),
      rows: rows.value,
      searchPlaceholder: isTemplateTab ? '搜索模板名称、编码或内容' : '搜索规则名称、事件编码或连接器',
      stats: [
        { key: 'total', label: '筛选结果', value: String(totalCount.value), tone: 'primary' },
        { key: 'page', label: '当前页记录', value: String(rows.value.length), tone: 'success' },
        {
          key: 'access',
          label: '当前权限',
          value: isTemplateTab
            ? canManageTemplate.value
              ? '可维护'
              : '只读'
            : canManageRule.value
              ? '可维护'
              : '只读',
          tone: 'neutral',
        },
      ],
      surface: 'list',
      tabs: [],
      tools: [],
    };
  });
  const remoteFilterValues = computed((): Readonly<Record<string, string>> => {
    if (activeTab.value === 'templates') {
      return { status: templateStatusFilter.value, type: templateTypeFilter.value };
    }
    return { status: ruleStatusFilter.value };
  });
  const remotePagination = computed(() => ({
    currentPage: currentPage.value,
    pageSize,
    searchQuery: keyword.value,
    totalCount: totalCount.value,
  }));
  const dialogTitle = computed(() => {
    if (dialogKind.value === 'template') return `${templateDraft.id ? '编辑' : '新建'}消息模板`;
    if (dialogKind.value === 'rule') return `${ruleDraft.id ? '编辑' : '新建'}通知规则`;
    return '只评估事件';
  });
  const dialogDescription = computed(() => {
    if (dialogKind.value === 'template') return '按后端模板字段保存；预览只渲染，不会调用外部连接器。';
    if (dialogKind.value === 'rule') return '事件编码由业务调用方提供。保存规则不会触发通知发送。';
    return '使用 commitTriggered=false 评估当前事件，不发送消息，也不更新时间。';
  });
  const toggleDescription = computed(() => {
    const target = toggleTarget.value;
    if (!target) return '';
    return target.status === 'Active'
      ? `确认停用“${target.name}”？停用后该规则不再参与后续通知评估。`
      : `确认启用“${target.name}”？启用后该规则会参与后续通知评估。`;
  });

  async function loadPage(): Promise<void> {
    activeRequest.value?.abort();
    const controller = new AbortController();
    activeRequest.value = controller;
    loading.value = true;
    loadError.value = '';
    try {
      if (activeTab.value === 'templates') {
        const result = await api.getTemplates(
          {
            keyword: keyword.value.trim() || undefined,
            maxResultCount: pageSize,
            messageType: templateTypeValues[templateTypeFilter.value],
            skipCount: (currentPage.value - 1) * pageSize,
            status: statusValues[templateStatusFilter.value] ?? 'All',
          },
          controller.signal,
        );
        if (controller.signal.aborted) return;
        templates.value = result.items;
        totalCount.value = result.totalCount;
      } else {
        const result = await api.getRules(
          {
            keyword: keyword.value.trim() || undefined,
            maxResultCount: pageSize,
            skipCount: (currentPage.value - 1) * pageSize,
            status: statusValues[ruleStatusFilter.value] ?? 'All',
          },
          controller.signal,
        );
        if (controller.signal.aborted) return;
        rules.value = result.items;
        totalCount.value = result.totalCount;
      }
    } catch (error) {
      if (controller.signal.aborted || (error instanceof Error && error.name === 'AbortError')) return;
      loadError.value = error instanceof Error ? error.message : '读取失败，请重试。';
    } finally {
      if (activeRequest.value === controller) loading.value = false;
    }
  }

  function selectTab(value: string): void {
    if (value === 'templates') void router.push({ name: routeNames.integrationTemplates });
    if (value === 'rules') void router.push({ name: routeNames.integrationRules });
  }

  function handleSearch(value: string): void {
    keyword.value = value;
    currentPage.value = 1;
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
    searchTimer.value = window.setTimeout(() => void loadPage(), 250);
  }

  function handleFilterChange(key: string, value: string): void {
    currentPage.value = 1;
    if (activeTab.value === 'templates' && key === 'status') templateStatusFilter.value = value;
    if (activeTab.value === 'templates' && key === 'type') templateTypeFilter.value = value;
    if (activeTab.value === 'rules' && key === 'status') ruleStatusFilter.value = value;
    void loadPage();
  }

  function changePage(page: number): void {
    currentPage.value = page;
    void loadPage();
  }

  function openTemplate(template?: IntegrationNotificationsApi.Template, readOnly = false): void {
    Object.assign(templateDraft, toTemplateDraft(template));
    previewResult.value = null;
    dialogError.value = '';
    dialogKind.value = 'template';
    dialogReadOnly.value = readOnly;
    dialogOpen.value = true;
  }

  function openRule(rule?: IntegrationNotificationsApi.Rule, readOnly = false): void {
    Object.assign(ruleDraft, toRuleDraft(rule));
    dialogError.value = '';
    dialogKind.value = 'rule';
    dialogReadOnly.value = readOnly;
    dialogOpen.value = true;
    void loadRuleOptions();
  }

  async function loadRuleOptions(): Promise<void> {
    optionError.value = '';
    templateOptions.value = [];
    connectorOptions.value = [];
    const tasks: Promise<void>[] = [];
    if (canViewTemplate.value) {
      tasks.push(
        api
          .getTemplates({ maxResultCount: 100, skipCount: 0, status: 'All' })
          .then((result) => {
            templateOptions.value = result.items.map((item) => ({
              label: `${item.name} · ${item.templateCode}${item.status === 'Active' ? '' : `（${templateStatusLabel(item.status)}）`}`,
              value: item.id,
            }));
          })
          .catch((error: unknown) => {
            optionError.value = error instanceof Error ? error.message : '模板选项读取失败。';
          }),
      );
    }
    if (canViewConnector.value) {
      tasks.push(
        connectorsApi
          .getConnectors({ includeDisabled: true })
          .then((result) => {
            connectorOptions.value = result.map((item) => ({
              label: `${item.displayName} · ${item.connectorCode}`,
              value: item.id,
            }));
          })
          .catch((error: unknown) => {
            optionError.value = error instanceof Error ? error.message : '连接器选项读取失败。';
          }),
      );
    }
    await Promise.all(tasks);
  }

  async function renderTemplatePreview(): Promise<void> {
    const error = validateTemplateDraft(templateDraft);
    if (error) {
      dialogError.value = error;
      return;
    }
    previewLoading.value = true;
    dialogError.value = '';
    try {
      previewResult.value = await api.renderTemplate({
        bodyTemplate: templateDraft.bodyTemplate,
        cardJson: templateDraft.cardJson,
        titleTemplate: templateDraft.titleTemplate,
        variablesJson: templateDraft.previewVariablesJson,
      });
    } catch (caught) {
      dialogError.value = caught instanceof Error ? caught.message : '模板预览失败。';
    } finally {
      previewLoading.value = false;
    }
  }

  async function submitDialog(): Promise<void> {
    dialogError.value = '';
    if (dialogKind.value === 'template') {
      if (!canManageTemplate.value || dialogReadOnly.value) return;
      const error = validateTemplateDraft(templateDraft);
      if (error) {
        dialogError.value = error;
        return;
      }
      saving.value = true;
      try {
        await api.saveTemplate({
          ...templateDraft,
          name: templateDraft.name.trim(),
          remark: templateDraft.remark.trim(),
          templateCode: templateDraft.templateCode.trim(),
          titleTemplate: templateDraft.titleTemplate,
        });
        toast.success('消息模板已保存。');
        dialogOpen.value = false;
        await loadPage();
      } catch (caught) {
        dialogError.value = caught instanceof Error ? caught.message : '消息模板保存失败。';
      } finally {
        saving.value = false;
      }
      return;
    }
    if (dialogKind.value !== 'rule' || !canManageRule.value || dialogReadOnly.value) return;
    const error = validateRuleDraft(ruleDraft);
    if (error) {
      dialogError.value = error;
      return;
    }
    saving.value = true;
    try {
      await api.saveRule(toRuleSaveInput(ruleDraft));
      toast.success('通知规则已保存。');
      dialogOpen.value = false;
      await loadPage();
    } catch (caught) {
      dialogError.value = caught instanceof Error ? caught.message : '通知规则保存失败。';
    } finally {
      saving.value = false;
    }
  }

  function openEvaluation(eventCode = ''): void {
    evaluationEventCode.value = eventCode;
    evaluationVariablesJson.value = '{}';
    evaluationResult.value = null;
    dialogError.value = '';
    dialogKind.value = 'evaluation';
    dialogReadOnly.value = false;
    dialogOpen.value = true;
  }

  async function evaluateEvent(): Promise<void> {
    if (!canManageRule.value) return;
    if (!evaluationEventCode.value.trim()) {
      dialogError.value = '请填写事件编码。';
      return;
    }
    try {
      const parsed: unknown = JSON.parse(evaluationVariablesJson.value || '{}');
      if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
        dialogError.value = '事件变量必须是JSON对象。';
        return;
      }
    } catch {
      dialogError.value = '事件变量必须是合法JSON对象。';
      return;
    }
    evaluationLoading.value = true;
    dialogError.value = '';
    try {
      evaluationResult.value = await api.evaluateRules({
        eventCode: evaluationEventCode.value.trim(),
        variablesJson: evaluationVariablesJson.value.trim() || '{}',
      });
    } catch (error) {
      dialogError.value = error instanceof Error ? error.message : '规则评估失败。';
    } finally {
      evaluationLoading.value = false;
    }
  }

  function startToggle(rowsToToggle: ListPageRow[]): void {
    if (!canManageRule.value) return;
    if (rowsToToggle.length !== 1) {
      toast.warning('请只选择一条通知规则。');
      return;
    }
    toggleTarget.value = rules.value.find((rule) => rule.id === rowsToToggle[0]?.id) ?? null;
    toggleDialogOpen.value = toggleTarget.value !== null;
  }

  async function confirmToggle(): Promise<void> {
    const rule = toggleTarget.value;
    if (!rule || !canManageRule.value) return;
    const status = rule.status === 'Active' ? 'Disabled' : 'Active';
    saving.value = true;
    try {
      await api.toggleRule(rule.id, status);
      toast.success(`通知规则已${ruleStatusLabel(status)}。`);
      toggleDialogOpen.value = false;
      toggleTarget.value = null;
      await loadPage();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : '通知规则状态更新失败。');
    } finally {
      saving.value = false;
    }
  }

  function handleAction(action: PageAction, selectedRows: ListPageRow[]): void {
    if (action.id === 'reload') void loadPage();
    if (action.id === 'create' && activeTab.value === 'templates' && canManageTemplate.value) openTemplate();
    if (action.id === 'create' && activeTab.value === 'rules' && canManageRule.value) openRule();
    if (action.id === 'edit' && activeTab.value === 'templates' && canManageTemplate.value) {
      if (selectedRows.length !== 1) return void toast.warning('请只选择一条消息模板。');
      const template = templates.value.find((item) => item.id === selectedRows[0]?.id);
      if (template) openTemplate(template);
    }
    if (action.id === 'edit' && activeTab.value === 'rules' && canManageRule.value) {
      if (selectedRows.length !== 1) return void toast.warning('请只选择一条通知规则。');
      const rule = rules.value.find((item) => item.id === selectedRows[0]?.id);
      if (rule) openRule(rule);
    }
    if (action.id === 'preview' && activeTab.value === 'templates' && canTestTemplate.value) {
      if (selectedRows.length !== 1) return void toast.warning('请只选择一条消息模板。');
      const template = templates.value.find((item) => item.id === selectedRows[0]?.id);
      if (template) openTemplate(template, true);
    }
    if (action.id === 'toggle') startToggle(selectedRows);
    if (action.id === 'evaluate') openEvaluation();
  }

  function openRow(row: ListPageRow): void {
    if (activeTab.value === 'templates') {
      const template = templates.value.find((item) => item.id === row.id);
      if (template) openTemplate(template, !canManageTemplate.value);
      return;
    }
    const rule = rules.value.find((item) => item.id === row.id);
    if (rule && canManageRule.value) openRule(rule);
  }

  function closeDialog(): void {
    if (saving.value || previewLoading.value || evaluationLoading.value) return;
    dialogOpen.value = false;
  }

  onMounted(() => void loadPage());
  onBeforeUnmount(() => {
    activeRequest.value?.abort();
    if (searchTimer.value !== undefined) window.clearTimeout(searchTimer.value);
  });
  watch(activeTab, () => {
    currentPage.value = 1;
    keyword.value = '';
    void loadPage();
  });

  return {
    activeTab,
    canManageRule,
    canManageTemplate,
    canTestTemplate,
    changePage,
    closeDialog,
    confirmToggle,
    connectorOptions,
    dialogDescription,
    dialogError,
    dialogKind,
    dialogOpen,
    dialogReadOnly,
    dialogTitle,
    evaluationEventCode,
    evaluationLoading,
    evaluationResult,
    evaluationVariablesJson,
    handleAction,
    handleFilterChange,
    handleSearch,
    loadError,
    loading,
    openEvaluation,
    openRow,
    optionError,
    pageModel,
    previewLoading,
    previewResult,
    remoteFilterValues,
    remotePagination,
    renderTemplatePreview,
    ruleDraft,
    saving,
    selectTab,
    submitDialog,
    templateDraft,
    templateOptions,
    toggleDescription,
    toggleDialogOpen,
    evaluateEvent,
  };
}
