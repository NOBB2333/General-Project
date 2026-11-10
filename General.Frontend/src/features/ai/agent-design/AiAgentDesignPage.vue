<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Network, Sparkles, UserRoundCog, Wrench } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import AiAgentsPanel from './AiAgentsPanel.vue';
import AiCommandsPanel from './AiCommandsPanel.vue';
import AiToolsPanel from './AiToolsPanel.vue';
import {
  toast,
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSwitch,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
} from '@/components/ui';

type DesignTab = 'agents' | 'commands' | 'personas' | 'skills' | 'tools';
type DeleteTarget = { kind: 'persona'; value: AiApi.Persona } | { kind: 'skill'; value: AiApi.Skill };

const api = createAiApi(apiClient);
const activeTab = ref<DesignTab>('agents');
const skills = ref<AiApi.Skill[]>([]);
const personas = ref<AiApi.Persona[]>([]);
const loadingSkills = ref(false);
const loadingPersonas = ref(false);
const saving = ref(false);
const deleting = ref(false);
const loadError = ref('');
const formError = ref('');
const actionError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const editingSkill = ref<AiApi.Skill | null>(null);
const editingPersona = ref<AiApi.Persona | null>(null);
const deleteTarget = ref<DeleteTarget | null>(null);
const canManage = computed(() => canAccess('Ai.Agent.Manage'));
const canUseTools = computed(() => canAccess('Ai.Conversation.Use'));

function blankSkill(): AiApi.SkillSaveInput {
  return {
    category: '项目管理',
    defaultMode: 'General',
    description: '',
    inputSchemaJson: '{}',
    instruction: '',
    isEnabled: true,
    name: '',
    outputSchemaJson: '{}',
    skillCode: '',
    sort: 0,
    toolCodes: [],
  };
}

function blankPersona(): AiApi.PersonaSaveInput {
  return {
    boundaryPrompt: '',
    configJson: '{}',
    description: '',
    isEnabled: true,
    name: '',
    personaCode: '',
    personalityPrompt: '',
    roleType: 'General',
    sort: 0,
    stylePrompt: '',
  };
}

const skillDraft = ref<AiApi.SkillSaveInput>(blankSkill());
const personaDraft = ref<AiApi.PersonaSaveInput>(blankPersona());

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function splitCodes(value: string): string[] {
  return [
    ...new Set(
      value
        .split(/[，,\n]/u)
        .map((item) => item.trim())
        .filter(Boolean),
    ),
  ];
}

function validJson(value: string): boolean {
  try {
    JSON.parse(value);
    return true;
  } catch {
    return false;
  }
}

const skillRows = computed<ListPageRow[]>(() =>
  skills.value.map((item) => ({
    category: item.category,
    defaultMode: item.defaultMode,
    id: item.id,
    name: item.name,
    skillCode: item.skillCode,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

const personaRows = computed<ListPageRow[]>(() =>
  personas.value.map((item) => ({
    id: item.id,
    name: item.name,
    personaCode: item.personaCode,
    roleType: item.roleType,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

function makePageModel(
  kind: DesignTab,
  rows: ListPageRow[],
  busy: boolean,
  total: number,
  enabled: number,
  builtin: number,
): ListPageModel {
  const isSkill = kind === 'skills';
  const noun = isSkill ? '技能' : '角色画像';
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: busy ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: `新增${noun}` },
      { id: 'edit', kind: 'custom', label: `编辑${noun}`, requiresSelection: true },
      { id: 'delete', kind: 'delete', label: `删除${noun}`, requiresSelection: true },
    );
  }
  return {
    actions,
    columns: isSkill
      ? [
          { key: 'name', title: '技能名称', width: 220, sticky: true },
          { key: 'skillCode', title: '技能编码', width: 220 },
          { key: 'category', title: '分类', width: 160 },
          { key: 'defaultMode', title: '默认模式', width: 120 },
          { key: 'status', title: '状态', width: 100, presentation: 'status' },
          { key: 'type', title: '来源', width: 100 },
        ]
      : [
          { key: 'name', title: '画像名称', width: 220, sticky: true },
          { key: 'personaCode', title: '画像编码', width: 220 },
          { key: 'roleType', title: '角色类型', width: 160 },
          { key: 'status', title: '状态', width: 100, presentation: 'status' },
          { key: 'type', title: '来源', width: 100 },
        ],
    demoPath: '/ai/agent-design',
    description: isSkill ? '维护会话和Agent可复用的技能指令。' : '维护Agent可关联的角色边界、个性和表达风格。',
    emptyHint: busy ? `正在读取${noun}…` : loadError.value ? `${noun}读取失败，请刷新重试。` : `暂无${noun}。`,
    filters: [],
    key: `AI管理|${noun}`,
    label: noun,
    listScopes: [{ id: 'all', label: `全部${noun}`, count: total }],
    listTabs: [{ id: 'all', label: `全部${noun}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: `ai-${kind}`,
    pageSize: 20,
    panels: [],
    path: '/ai/agent-design',
    primaryAction: `新增${noun}`,
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-agent-design',
    rows,
    searchPlaceholder: `搜索${noun}名称、编码或说明`,
    stats: [
      { key: 'total', label: `全部${noun}`, value: String(total), tone: 'primary' },
      { key: 'enabled', label: '启用', value: String(enabled), tone: 'success' },
      { key: 'builtin', label: '内置', value: String(builtin), tone: 'warning' },
      { key: 'custom', label: '自定义', value: String(total - builtin), tone: 'neutral' },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const skillPageModel = computed(() =>
  makePageModel(
    'skills',
    skillRows.value,
    loadingSkills.value,
    skills.value.length,
    skills.value.filter((item) => item.isEnabled).length,
    skills.value.filter((item) => item.isBuiltin).length,
  ),
);
const personaPageModel = computed(() =>
  makePageModel(
    'personas',
    personaRows.value,
    loadingPersonas.value,
    personas.value.length,
    personas.value.filter((item) => item.isEnabled).length,
    personas.value.filter((item) => item.isBuiltin).length,
  ),
);

async function loadSkills(): Promise<void> {
  loadingSkills.value = true;
  loadError.value = '';
  try {
    skills.value = await api.getSkills({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    loadingSkills.value = false;
  }
}

async function loadPersonas(): Promise<void> {
  loadingPersonas.value = true;
  loadError.value = '';
  try {
    personas.value = await api.getPersonas({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    loadingPersonas.value = false;
  }
}

function openSkillForm(item: AiApi.Skill | null): void {
  editingSkill.value = item;
  editingPersona.value = null;
  formError.value = '';
  skillDraft.value = item
    ? {
        category: item.category,
        defaultMode: item.defaultMode,
        description: item.description,
        inputSchemaJson: item.inputSchemaJson,
        instruction: item.instruction,
        isEnabled: item.isEnabled,
        name: item.name,
        outputSchemaJson: item.outputSchemaJson,
        skillCode: item.skillCode,
        sort: item.sort,
        toolCodes: [...item.toolCodes],
      }
    : blankSkill();
  dialogOpen.value = true;
}

function openPersonaForm(item: AiApi.Persona | null): void {
  editingPersona.value = item;
  editingSkill.value = null;
  formError.value = '';
  personaDraft.value = item
    ? {
        boundaryPrompt: item.boundaryPrompt,
        configJson: item.configJson,
        description: item.description,
        isEnabled: item.isEnabled,
        name: item.name,
        personaCode: item.personaCode,
        personalityPrompt: item.personalityPrompt,
        roleType: item.roleType,
        sort: item.sort,
        stylePrompt: item.stylePrompt,
      }
    : blankPersona();
  dialogOpen.value = true;
}

function selectedSkill(selection: ListPageRow[]): AiApi.Skill | null {
  const selected = selection.length === 1 ? skills.value.find((item) => item.id === selection[0]?.id) : null;
  if (!selected) actionError.value = '请选择一条技能记录。';
  else actionError.value = '';
  return selected ?? null;
}

function selectedPersona(selection: ListPageRow[]): AiApi.Persona | null {
  const selected = selection.length === 1 ? personas.value.find((item) => item.id === selection[0]?.id) : null;
  if (!selected) actionError.value = '请选择一条角色画像。';
  else actionError.value = '';
  return selected ?? null;
}

function handleSkillAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') return void loadSkills();
  if (action.id === 'create') return openSkillForm(null);
  const selected = selectedSkill(selection);
  if (!selected) return;
  if (action.id === 'edit') openSkillForm(selected);
  if (action.id === 'delete') {
    if (selected.isBuiltin) return void toast.warning('内置技能不能删除，可停用或调整配置。');
    deleteTarget.value = { kind: 'skill', value: selected };
    deleteDialogOpen.value = true;
  }
}

function handlePersonaAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') return void loadPersonas();
  if (action.id === 'create') return openPersonaForm(null);
  const selected = selectedPersona(selection);
  if (!selected) return;
  if (action.id === 'edit') openPersonaForm(selected);
  if (action.id === 'delete') {
    if (selected.isBuiltin) return void toast.warning('内置角色画像不能删除，可停用或调整配置。');
    deleteTarget.value = { kind: 'persona', value: selected };
    deleteDialogOpen.value = true;
  }
}

function handleSkillRow(row: ListPageRow): void {
  const item = skills.value.find((skill) => skill.id === row.id);
  if (item && canManage.value) openSkillForm(item);
}

function handlePersonaRow(row: ListPageRow): void {
  const item = personas.value.find((persona) => persona.id === row.id);
  if (item && canManage.value) openPersonaForm(item);
}

async function save(): Promise<void> {
  if (saving.value) return;
  formError.value = '';
  if (editingSkill.value || (!editingPersona.value && activeTab.value === 'skills')) {
    const input = skillDraft.value;
    if (!input.skillCode.trim() || !input.name.trim() || !input.instruction.trim()) {
      formError.value = '技能编码、名称和指令为必填项。';
      return;
    }
    if (!validJson(input.inputSchemaJson) || !validJson(input.outputSchemaJson)) {
      formError.value = '技能输入和输出 Schema 必须是合法 JSON。';
      return;
    }
    saving.value = true;
    try {
      if (editingSkill.value) await api.updateSkill(editingSkill.value.id, input);
      else await api.createSkill(input);
      dialogOpen.value = false;
      await loadSkills();
      toast.success('技能配置已保存。');
    } catch (error) {
      formError.value = errorText(error);
    } finally {
      saving.value = false;
    }
    return;
  }

  const input = personaDraft.value;
  if (!input.personaCode.trim() || !input.name.trim() || !input.personalityPrompt.trim()) {
    formError.value = '画像编码、名称和个性提示词为必填项。';
    return;
  }
  if (!validJson(input.configJson)) {
    formError.value = '角色配置必须是合法 JSON。';
    return;
  }
  saving.value = true;
  try {
    if (editingPersona.value) await api.updatePersona(editingPersona.value.id, input);
    else await api.createPersona(input);
    dialogOpen.value = false;
    await loadPersonas();
    toast.success('角色画像已保存。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

async function confirmDelete(): Promise<void> {
  if (!deleteTarget.value || deleting.value) return;
  deleting.value = true;
  const target = deleteTarget.value;
  try {
    if (target.kind === 'skill') await api.deleteSkill(target.value.id);
    else await api.deletePersona(target.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    if (target.kind === 'skill') await loadSkills();
    else await loadPersonas();
    toast.success(target.kind === 'skill' ? '技能配置已删除。' : '角色画像已删除。');
  } catch (error) {
    actionError.value = errorText(error);
    toast.error(actionError.value);
  } finally {
    deleting.value = false;
  }
}

onMounted(() => {
  void loadSkills();
  void loadPersonas();
});
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="Agent编排管理">
      <UiTabsTrigger
        v-if="canManage"
        value="agents"
        >Agent</UiTabsTrigger
      >
      <UiTabsTrigger
        v-if="canManage"
        value="commands"
        >命令</UiTabsTrigger
      >
      <UiTabsTrigger value="skills"
        ><Sparkles
          :size="15"
          aria-hidden="true"
        />技能</UiTabsTrigger
      >
      <UiTabsTrigger value="personas"
        ><UserRoundCog
          :size="15"
          aria-hidden="true"
        />角色画像</UiTabsTrigger
      >
      <UiTabsTrigger
        v-if="canUseTools"
        value="tools"
        ><Wrench
          :size="15"
          aria-hidden="true"
        />工具与记录</UiTabsTrigger
      >
    </UiTabsList>
    <UiTabPanel
      v-if="canManage"
      value="agents"
    >
      <AiAgentsPanel />
    </UiTabPanel>
    <UiTabPanel
      v-if="canManage"
      value="commands"
    >
      <AiCommandsPanel />
    </UiTabPanel>
    <UiTabPanel value="skills">
      <ListPageSurface
        :model="skillPageModel"
        @action="handleSkillAction"
        @row="handleSkillRow"
      >
        <template #hero-icon
          ><Network
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>技能指令只对 Agent 管理权限开放；会话使用者只能读取启用技能的安全摘要。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
          <p
            v-if="actionError"
            role="alert"
          >
            {{ actionError }}
          </p>
          <p
            v-if="!canManage"
            role="alert"
          >
            当前账号没有 Agent 管理权限。
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="personas">
      <ListPageSurface
        :model="personaPageModel"
        @action="handlePersonaAction"
        @row="handlePersonaRow"
      >
        <template #hero-icon
          ><UserRoundCog
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>角色画像由Agent定义引用；运行计划预览会回显Agent实际绑定的角色画像编码。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
          <p
            v-if="actionError"
            role="alert"
          >
            {{ actionError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel
      v-if="canUseTools"
      value="tools"
    >
      <AiToolsPanel />
    </UiTabPanel>
  </UiTabs>

  <UiDialog
    :open="dialogOpen"
    :title="
      editingSkill
        ? `编辑技能：${editingSkill.name}`
        : editingPersona
          ? `编辑角色画像：${editingPersona.name}`
          : activeTab === 'skills'
            ? '新增技能'
            : '新增角色画像'
    "
    description="管理内容仅通过Agent管理权限读取；保存后才会更新服务端配置。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form
      v-if="editingSkill || (!editingPersona && activeTab === 'skills')"
      @submit.prevent="save"
    >
      <UiFormField label="技能名称"
        ><UiInput
          v-model="skillDraft.name"
          maxlength="128"
          required
      /></UiFormField>
      <UiFormField label="技能编码"
        ><UiInput
          v-model="skillDraft.skillCode"
          maxlength="128"
          required
          :readonly="Boolean(editingSkill)"
      /></UiFormField>
      <UiFormField label="分类"
        ><UiInput
          v-model="skillDraft.category"
          maxlength="64"
      /></UiFormField>
      <UiFormField label="默认模式"
        ><UiInput
          v-model="skillDraft.defaultMode"
          maxlength="32"
      /></UiFormField>
      <UiFormField label="排序"
        ><UiInput
          :model-value="String(skillDraft.sort)"
          type="number"
          @update:model-value="skillDraft.sort = Number($event)"
      /></UiFormField>
      <UiFormField label="技能状态"
        ><UiSwitch
          v-model="skillDraft.isEnabled"
          label="启用技能"
      /></UiFormField>
      <UiFormField
        label="说明"
        wide
        ><UiTextarea
          v-model="skillDraft.description"
          rows="2"
          maxlength="512"
      /></UiFormField>
      <UiFormField
        label="技能指令"
        wide
        ><UiTextarea
          v-model="skillDraft.instruction"
          rows="7"
          maxlength="16000"
          required
      /></UiFormField>
      <UiFormField
        label="工具编码"
        hint="多个编码用逗号或换行分隔。"
        wide
      >
        <UiTextarea
          :model-value="skillDraft.toolCodes.join('\n')"
          rows="3"
          @update:model-value="skillDraft.toolCodes = splitCodes($event)"
        />
      </UiFormField>
      <UiFormField
        label="输入 Schema JSON"
        wide
        ><UiTextarea
          v-model="skillDraft.inputSchemaJson"
          rows="4"
          maxlength="8000"
      /></UiFormField>
      <UiFormField
        label="输出 Schema JSON"
        wide
        ><UiTextarea
          v-model="skillDraft.outputSchemaJson"
          rows="4"
          maxlength="8000"
      /></UiFormField>
    </form>
    <form
      v-else
      @submit.prevent="save"
    >
      <UiFormField label="角色画像名称"
        ><UiInput
          v-model="personaDraft.name"
          maxlength="128"
          required
      /></UiFormField>
      <UiFormField label="画像编码"
        ><UiInput
          v-model="personaDraft.personaCode"
          maxlength="128"
          required
          :readonly="Boolean(editingPersona)"
      /></UiFormField>
      <UiFormField label="角色类型"
        ><UiInput
          v-model="personaDraft.roleType"
          maxlength="32"
      /></UiFormField>
      <UiFormField label="排序"
        ><UiInput
          :model-value="String(personaDraft.sort)"
          type="number"
          @update:model-value="personaDraft.sort = Number($event)"
      /></UiFormField>
      <UiFormField label="画像状态"
        ><UiSwitch
          v-model="personaDraft.isEnabled"
          label="启用角色画像"
      /></UiFormField>
      <UiFormField
        label="说明"
        wide
        ><UiTextarea
          v-model="personaDraft.description"
          rows="2"
          maxlength="512"
      /></UiFormField>
      <UiFormField
        label="个性提示词"
        wide
        ><UiTextarea
          v-model="personaDraft.personalityPrompt"
          rows="5"
          maxlength="8000"
          required
      /></UiFormField>
      <UiFormField
        label="边界提示词"
        wide
        ><UiTextarea
          v-model="personaDraft.boundaryPrompt"
          rows="4"
          maxlength="8000"
      /></UiFormField>
      <UiFormField
        label="表达风格提示词"
        wide
        ><UiTextarea
          v-model="personaDraft.stylePrompt"
          rows="4"
          maxlength="4000"
      /></UiFormField>
      <UiFormField
        label="配置 JSON"
        wide
        ><UiTextarea
          v-model="personaDraft.configJson"
          rows="4"
          maxlength="8000"
      /></UiFormField>
    </form>
    <p
      v-if="formError"
      role="alert"
    >
      {{ formError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="save"
        >{{ saving ? '保存中…' : '保存配置' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除配置"
    :description="deleteTarget ? `删除“${deleteTarget.value.name}”？此操作无法撤销。` : ''"
    confirm-label="删除"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
