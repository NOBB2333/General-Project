<script setup lang="ts">
import { PlugZap } from 'lucide-vue-next';
import { ListPageSurface } from '@/components/page';
import type { UiSelectOption } from '@/components/ui';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
} from '@/components/ui';
import type { NotificationTab } from './notifications.model';
import { useIntegrationNotifications } from './useIntegrationNotifications';

const props = defineProps<{ initialTab: NotificationTab }>();
const page = useIntegrationNotifications(props);
const {
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
  evaluateEvent,
  handleAction,
  handleFilterChange,
  handleSearch,
  loadError,
  loading,
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
} = page;

const templateTypeSelectOptions: readonly UiSelectOption[] = [
  { label: '文本', value: 'Text' },
  { label: 'Markdown', value: 'Markdown' },
  { label: '卡片', value: 'Card' },
];
const templateStatusSelectOptions: readonly UiSelectOption[] = [
  { label: '草稿', value: 'Draft' },
  { label: '启用', value: 'Active' },
  { label: '停用', value: 'Disabled' },
];
const ruleStatusSelectOptions: readonly UiSelectOption[] = [
  { label: '启用', value: 'Active' },
  { label: '停用', value: 'Disabled' },
  { label: '暂停', value: 'Paused' },
];
const channelSelectOptions: readonly UiSelectOption[] = [
  { label: '个人消息', value: 'Personal' },
  { label: '群消息', value: 'Group' },
  { label: '个人与群', value: 'Mixed' },
];

function closeToggleDialog() {
  if (saving.value) return;
  toggleDialogOpen.value = false;
}
</script>

<template>
  <ListPageSurface
    :key="activeTab"
    :model="pageModel"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @filter-change="handleFilterChange"
    @page-change="changePage"
    @row="openRow"
    @search-change="handleSearch"
  >
    <template #hero-icon>
      <PlugZap
        :size="24"
        :stroke-width="1.7"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <div>
        <UiTabs
          :model-value="activeTab"
          @update:model-value="selectTab"
        >
          <UiTabsList label="通知模板和规则">
            <UiTabsTrigger value="templates">消息模板</UiTabsTrigger>
            <UiTabsTrigger value="rules">通知规则</UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
        <p
          v-if="loading"
          role="status"
        >
          正在读取当前列表…
        </p>
        <p
          v-else-if="loadError"
          role="alert"
        >
          {{ loadError }}
        </p>
        <p v-if="activeTab === 'rules'">
          事件编码需与后端实际发出的事件一致；当前服务没有事件目录接口。规则评估只做判断，不发送消息。
        </p>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="dialogOpen"
    :title="dialogTitle"
    :description="dialogDescription"
    size="xl"
    @close="closeDialog"
  >
    <section v-if="dialogKind === 'template'">
      <UiFormField
        label="模板名称"
        required
      >
        <UiInput
          v-model="templateDraft.name"
          :disabled="dialogReadOnly"
          maxlength="128"
          aria-label="模板名称"
          placeholder="输入模板名称"
        />
      </UiFormField>
      <UiFormField
        label="模板编码"
        required
      >
        <UiInput
          v-model="templateDraft.templateCode"
          :disabled="dialogReadOnly"
          maxlength="64"
          aria-label="模板编码"
          placeholder="输入稳定且唯一的模板编码"
        />
      </UiFormField>
      <UiFormField label="消息类型">
        <UiSelect
          :model-value="templateDraft.messageType"
          :options="templateTypeSelectOptions"
          label="消息类型"
          :disabled="dialogReadOnly"
          @update:model-value="templateDraft.messageType = $event"
        />
      </UiFormField>
      <UiFormField label="模板状态">
        <UiSelect
          :model-value="templateDraft.status"
          :options="templateStatusSelectOptions"
          label="模板状态"
          :disabled="dialogReadOnly"
          @update:model-value="templateDraft.status = $event"
        />
      </UiFormField>
      <UiFormField label="标题模板">
        <UiInput
          v-model="templateDraft.titleTemplate"
          :disabled="dialogReadOnly"
          maxlength="256"
          aria-label="标题模板"
          placeholder="例如：项目 {{project.name}} 状态更新"
        />
      </UiFormField>
      <UiFormField label="正文模板">
        <UiTextarea
          v-model="templateDraft.bodyTemplate"
          :disabled="dialogReadOnly"
          rows="5"
          aria-label="正文模板"
          placeholder="输入正文模板，可使用 {{变量路径}}"
        />
      </UiFormField>
      <UiFormField label="卡片 JSON">
        <UiTextarea
          v-model="templateDraft.cardJson"
          :disabled="dialogReadOnly"
          rows="5"
          spellcheck="false"
          aria-label="卡片 JSON"
        />
      </UiFormField>
      <UiFormField label="变量说明 JSON">
        <UiTextarea
          v-model="templateDraft.variableSchemaJson"
          :disabled="dialogReadOnly"
          rows="4"
          spellcheck="false"
          aria-label="变量说明 JSON"
        />
      </UiFormField>
      <UiFormField label="预览变量 JSON">
        <UiTextarea
          v-model="templateDraft.previewVariablesJson"
          :disabled="dialogReadOnly"
          rows="4"
          spellcheck="false"
          aria-label="预览变量 JSON"
        />
      </UiFormField>
      <UiFormField label="备注">
        <UiInput
          v-model="templateDraft.remark"
          :disabled="dialogReadOnly"
          maxlength="512"
          aria-label="模板备注"
        />
      </UiFormField>
      <p>
        变量用 {{ '{' }}{{ '{' }}变量路径{{ '}' }}{{ '}' }} 表示；渲染结果由后端生成，金额、成本和利润等敏感字段会脱敏。
      </p>
      <p
        v-if="dialogError"
        role="alert"
      >
        {{ dialogError }}
      </p>
      <div>
        <UiButton
          v-if="canTestTemplate"
          :loading="previewLoading"
          @click="renderTemplatePreview"
        >
          预览内容
        </UiButton>
      </div>
      <section
        v-if="previewResult"
        aria-label="模板预览结果"
      >
        <h3>预览结果</h3>
        <p>标题：{{ previewResult.renderedTitle || '（空）' }}</p>
        <p>正文：</p>
        <pre>{{ previewResult.renderedBody || '（空）' }}</pre>
        <p>渲染卡片 JSON：</p>
        <pre>{{ previewResult.renderedCardJson }}</pre>
        <p v-if="previewResult.missingVariables.length">缺少变量：{{ previewResult.missingVariables.join('、') }}</p>
      </section>
    </section>

    <section v-else-if="dialogKind === 'rule'">
      <UiFormField
        label="规则名称"
        required
      >
        <UiInput
          v-model="ruleDraft.name"
          :disabled="dialogReadOnly"
          maxlength="128"
          aria-label="规则名称"
        />
      </UiFormField>
      <UiFormField
        label="规则编码"
        required
      >
        <UiInput
          v-model="ruleDraft.ruleCode"
          :disabled="dialogReadOnly"
          maxlength="64"
          aria-label="规则编码"
        />
      </UiFormField>
      <UiFormField
        label="真实事件编码"
        required
        hint="此服务未提供事件目录，请填入业务调用方实际发送的事件编码。"
      >
        <UiInput
          v-model="ruleDraft.eventCode"
          :disabled="dialogReadOnly"
          maxlength="128"
          aria-label="真实事件编码"
        />
      </UiFormField>
      <UiFormField label="事件名称">
        <UiInput
          v-model="ruleDraft.eventName"
          :disabled="dialogReadOnly"
          maxlength="128"
          aria-label="事件名称"
        />
      </UiFormField>
      <UiFormField
        label="消息模板"
        required
        hint="有模板查看权限时可从列表选择；否则填模板 GUID。模板必须处于启用状态才会通过规则评估。"
      >
        <UiSelect
          v-if="templateOptions.length"
          :model-value="ruleDraft.templateId"
          :options="templateOptions"
          label="消息模板"
          :disabled="dialogReadOnly"
          @update:model-value="ruleDraft.templateId = $event"
        />
        <UiInput
          v-else
          v-model="ruleDraft.templateId"
          :disabled="dialogReadOnly"
          aria-label="消息模板 GUID"
          placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        />
      </UiFormField>
      <UiFormField label="消息渠道">
        <UiSelect
          :model-value="ruleDraft.channelType"
          :options="channelSelectOptions"
          label="消息渠道"
          :disabled="dialogReadOnly"
          @update:model-value="ruleDraft.channelType = $event"
        />
      </UiFormField>
      <UiFormField label="连接器">
        <UiSelect
          v-if="connectorOptions.length"
          :model-value="ruleDraft.connectorId"
          :options="[{ label: '不指定', value: '' }, ...connectorOptions]"
          label="连接器"
          :disabled="dialogReadOnly"
          @update:model-value="ruleDraft.connectorId = $event"
        />
        <UiInput
          v-else
          v-model="ruleDraft.connectorId"
          :disabled="dialogReadOnly"
          aria-label="连接器 GUID"
          placeholder="可留空，或填写连接器 GUID"
        />
      </UiFormField>
      <UiFormField label="目标类型">
        <UiInput
          v-model="ruleDraft.targetType"
          :disabled="dialogReadOnly"
          maxlength="32"
          aria-label="目标类型"
          placeholder="OpenId"
        />
      </UiFormField>
      <UiFormField label="规则状态">
        <UiSelect
          :model-value="ruleDraft.status"
          :options="ruleStatusSelectOptions"
          label="规则状态"
          :disabled="dialogReadOnly"
          @update:model-value="ruleDraft.status = $event"
        />
      </UiFormField>
      <UiFormField
        label="接收人规则 JSON"
        hint="Direct 示例：{'mode':'Direct','targetType':'OpenId','targetIds':['external-id']}；群绑定可用 ProjectGroup 和 project.id 路径。"
      >
        <UiTextarea
          v-model="ruleDraft.recipientRuleJson"
          :disabled="dialogReadOnly"
          rows="5"
          spellcheck="false"
          aria-label="接收人规则 JSON"
        />
      </UiFormField>
      <UiFormField
        label="触发条件 JSON"
        hint="支持 equals、notEquals、contains、min、max；键为事件变量路径，例如 {'equals':{'project.status':'AtRisk'}}。"
      >
        <UiTextarea
          v-model="ruleDraft.conditionJson"
          :disabled="dialogReadOnly"
          rows="5"
          spellcheck="false"
          aria-label="触发条件 JSON"
        />
      </UiFormField>
      <UiFormField label="静默开始时间">
        <UiInput
          v-model="ruleDraft.quietStartTime"
          :disabled="dialogReadOnly"
          placeholder="例如 22:00，留空表示不限制"
          aria-label="静默开始时间"
        />
      </UiFormField>
      <UiFormField label="静默结束时间">
        <UiInput
          v-model="ruleDraft.quietEndTime"
          :disabled="dialogReadOnly"
          placeholder="例如 08:00，留空表示不限制"
          aria-label="静默结束时间"
        />
      </UiFormField>
      <UiFormField label="频控窗口（分钟）">
        <UiInput
          v-model="ruleDraft.rateLimitWindowMinutes"
          :disabled="dialogReadOnly"
          type="number"
          min="0"
          aria-label="频控窗口分钟"
        />
      </UiFormField>
      <UiFormField
        label="窗口内最多触发次数"
        hint="当前后端只实现 0（关闭）或 1（窗口内已触发则拦截）。"
      >
        <UiInput
          v-model="ruleDraft.rateLimitMaxCount"
          :disabled="dialogReadOnly"
          type="number"
          min="0"
          max="1"
          aria-label="窗口内最多触发次数"
        />
      </UiFormField>
      <UiFormField label="备注">
        <UiInput
          v-model="ruleDraft.remark"
          :disabled="dialogReadOnly"
          maxlength="512"
          aria-label="规则备注"
        />
      </UiFormField>
      <p
        v-if="optionError"
        role="alert"
      >
        关联选项读取失败：{{ optionError }}
      </p>
      <p
        v-if="dialogError"
        role="alert"
      >
        {{ dialogError }}
      </p>
    </section>

    <section v-else-if="dialogKind === 'evaluation'">
      <p>只调用规则 evaluate 接口，固定 commitTriggered=false；不会发送消息或写入规则最近触发时间。</p>
      <UiFormField
        label="事件编码"
        required
      >
        <UiInput
          v-model="evaluationEventCode"
          maxlength="128"
          aria-label="事件编码"
          placeholder="填写后端实际使用的事件编码"
        />
      </UiFormField>
      <UiFormField
        label="事件变量 JSON"
        hint="根节点必须是对象；请勿在评估数据中放入不必要的敏感信息。"
      >
        <UiTextarea
          v-model="evaluationVariablesJson"
          rows="8"
          spellcheck="false"
          aria-label="事件变量 JSON"
        />
      </UiFormField>
      <p
        v-if="dialogError"
        role="alert"
      >
        {{ dialogError }}
      </p>
      <UiButton
        variant="primary"
        :loading="evaluationLoading"
        @click="evaluateEvent"
      >
        执行只读评估
      </UiButton>
      <section
        v-if="evaluationResult"
        aria-label="通知规则评估结果"
      >
        <h3>评估结果</h3>
        <p>
          事件：{{ evaluationResult.eventCode }}；匹配规则：{{ evaluationResult.totalRuleCount }}；可通过：{{
            evaluationResult.readyCount
          }}；拦截：{{ evaluationResult.blockedCount }}
        </p>
        <article
          v-for="item in evaluationResult.items"
          :key="item.ruleId"
        >
          <h4>{{ item.ruleName }} · {{ item.canSend ? '评估通过' : '已拦截' }}</h4>
          <p v-if="item.blockReason">原因：{{ item.blockReason }}</p>
          <p v-if="item.missingVariables.length">缺少变量：{{ item.missingVariables.join('、') }}</p>
          <p>标题：{{ item.renderedTitle || '（空）' }}</p>
          <p>正文：{{ item.renderedBody || '（空）' }}</p>
        </article>
      </section>
    </section>

    <template #footer>
      <UiButton
        :disabled="saving || previewLoading || evaluationLoading"
        @click="closeDialog"
      >
        关闭
      </UiButton>
      <UiButton
        v-if="dialogKind === 'template' && canManageTemplate && !dialogReadOnly"
        variant="primary"
        :loading="saving"
        @click="submitDialog"
      >
        保存
      </UiButton>
      <UiButton
        v-if="dialogKind === 'rule' && canManageRule && !dialogReadOnly"
        variant="primary"
        :loading="saving"
        @click="submitDialog"
      >
        保存规则
      </UiButton>
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="toggleDialogOpen"
    title="确认调整规则状态"
    :description="toggleDescription"
    size="sm"
    @close="closeToggleDialog"
  >
    <p>{{ toggleDescription }}</p>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="closeToggleDialog"
      >
        取消
      </UiButton>
      <UiButton
        variant="primary"
        :loading="saving"
        @click="confirmToggle"
      >
        确认
      </UiButton>
    </template>
  </UiDialog>
</template>
