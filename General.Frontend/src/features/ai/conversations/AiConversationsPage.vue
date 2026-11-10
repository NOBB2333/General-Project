<script setup lang="ts">
import { Archive, Download, RefreshCw, RotateCcw, Sparkles, Trash2 } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import FeishuOrdersPage from '@/features/sales/views/FeishuOrdersPage.vue';
import FeishuChatPanel from '@/features/sales/views/feishu-orders/FeishuChatPanel.vue';
import FeishuConversationList from '@/features/sales/views/feishu-orders/FeishuConversationList.vue';
import FeishuOrdersHero from '@/features/sales/views/feishu-orders/FeishuOrdersHero.vue';
import { useAiConversations } from './useAiConversations';

const ai = useAiConversations();
const pageTitle = computed(() => (ai.managementMode.value ? '会话管理' : 'AI 对话'));
const pagePath = computed(() => (ai.managementMode.value ? '/ai/conversation-management' : '/ai/conversations'));
const noticeText = computed(() => ai.notice.value || ai.error.value || ai.modelError.value || ai.useCatalogError.value);
const selectedTemplate = computed(() =>
  ai.usableTemplates.value.find((item) => item.id === ai.selectedTemplateId.value),
);

function togglePlanStep(payload: { isDone: boolean; messageId: string; stepId: string }) {
  return ai.runPlanMutation(payload.messageId, 'toggle', { isDone: payload.isDone, stepId: payload.stepId });
}
</script>

<template>
  <FeishuOrdersPage
    mode="ai"
    :page-label="pageTitle"
    page-module="AI 管理"
    :page-route="pagePath"
    :conversation-count="ai.conversations.value.length"
    :message-count="ai.displayMessages.value.length"
  >
    <template #hero>
      <FeishuOrdersHero
        mode="ai"
        :title="pageTitle"
        :subtitle="
          ai.managementMode.value
            ? '查看、归档、恢复、导出或删除个人 AI 会话。'
            : '与 AI 助手对话，计划步骤和工具执行结果以服务端状态呈现。'
        "
      />
    </template>
    <template #workspace>
      <FeishuConversationList
        mode="ai"
        :conversations="ai.conversationItems.value"
        :active-id="ai.activeConversationId.value"
        @select="ai.selectConversation"
        @create="ai.createConversation()"
      />
      <FeishuChatPanel
        mode="ai"
        :conversation="ai.activeConversation.value"
        :messages="ai.displayMessages.value"
        :step="0"
        :busy="ai.isSending.value || ai.usingTemplate.value"
        :model-name="ai.headerModelName.value"
        :notice="noticeText"
        :readonly="ai.readonly.value"
        @send="ai.sendMessage"
        @retry="ai.sendMessage"
        @cancel="ai.stopMessage"
        @confirm-plan="ai.runPlanMutation($event, 'confirm')"
        @create-plan-draft="ai.createPlanDraft"
        @toggle-plan-step="togglePlanStep"
      >
        <template #header-actions>
          <UiSelect
            :model-value="ai.newConversationMode.value"
            :options="ai.modeOptions"
            label="新会话模式"
            size="sm"
            :disabled="ai.isSending.value"
            @update:model-value="ai.setNewConversationMode"
          />
          <UiSelect
            v-if="ai.models.value.length"
            :model-value="ai.selectedModelId.value"
            :options="ai.modelOptions.value"
            label="AI 模型"
            size="sm"
            :disabled="ai.isSending.value || ai.readonly.value"
            @update:model-value="ai.updateSelectedModel"
          />
          <UiSelect
            v-if="!ai.managementMode.value && ai.skillOptions.value.length"
            :model-value="ai.selectedSkillCode.value"
            :options="ai.skillOptions.value"
            label="使用技能"
            size="sm"
            :disabled="ai.isSending.value || ai.usingTemplate.value || ai.readonly.value"
            @update:model-value="ai.selectedSkillCode.value = $event"
          />
          <UiButton
            v-if="ai.canStartFromTemplate.value"
            variant="secondary"
            size="sm"
            :disabled="ai.isSending.value || ai.usingTemplate.value"
            @click="ai.openTemplateDialog"
          >
            <template #leading
              ><Sparkles
                :size="15"
                aria-hidden="true"
            /></template>
            从场景开始
          </UiButton>
          <UiButton
            variant="ghost"
            size="icon-sm"
            title="重新读取服务端会话"
            :disabled="ai.loading.value || ai.isSending.value"
            @click="ai.refresh"
          >
            <template #leading
              ><RefreshCw
                :size="15"
                :stroke-width="1.8"
            /></template>
          </UiButton>
          <UiButton
            v-if="ai.detail.value"
            variant="ghost"
            size="icon-sm"
            :title="ai.isArchived.value ? '恢复会话' : '归档会话'"
            :disabled="ai.isSending.value"
            @click="ai.toggleArchive"
          >
            <template #leading>
              <RotateCcw
                v-if="ai.isArchived.value"
                :size="15"
                :stroke-width="1.8"
              />
              <Archive
                v-else
                :size="15"
                :stroke-width="1.8"
              />
            </template>
          </UiButton>
          <UiButton
            v-if="ai.detail.value"
            variant="ghost"
            size="icon-sm"
            title="导出 Markdown"
            :disabled="ai.isSending.value"
            @click="ai.exportConversation"
          >
            <template #leading
              ><Download
                :size="15"
                :stroke-width="1.8"
            /></template>
          </UiButton>
          <UiButton
            v-if="ai.detail.value"
            variant="ghost"
            size="icon-sm"
            title="删除会话"
            :disabled="ai.isSending.value"
            @click="ai.requestDeleteConversation"
          >
            <template #leading
              ><Trash2
                :size="15"
                :stroke-width="1.8"
            /></template>
          </UiButton>
        </template>
      </FeishuChatPanel>
    </template>
    <template #dialogs>
      <UiAlertDialog
        v-model:open="ai.deleteDialogOpen.value"
        title="删除 AI 会话"
        description="删除后该会话及全部消息无法恢复。"
        confirm-label="删除会话"
        @confirm="ai.deleteConversation"
      />
      <UiDialog
        v-model:open="ai.templateDialogOpen.value"
        title="从场景创建会话"
        description="这会创建一条真实会话，并将模板提示词和输入发送给模型；不会只做预览。"
        size="lg"
      >
        <UiFormField label="场景模板">
          <UiSelect
            :model-value="ai.selectedTemplateId.value"
            :options="ai.templateOptions.value"
            label="场景模板"
            @update:model-value="ai.selectedTemplateId.value = $event"
          />
        </UiFormField>
        <p v-if="selectedTemplate?.description">{{ selectedTemplate.description }}</p>
        <UiFormField
          v-if="selectedTemplate && selectedTemplate.inputSchemaJson !== '{}'"
          label="输入字段说明"
        >
          <UiTextarea
            :model-value="selectedTemplate.inputSchemaJson"
            rows="4"
            readonly
          />
        </UiFormField>
        <UiFormField
          label="会话标题"
          hint="留空时由服务端按场景名称生成。"
        >
          <UiInput
            v-model="ai.templateTitle.value"
            maxlength="128"
          />
        </UiFormField>
        <UiFormField
          label="输入 JSON"
          hint="按场景模板要求提供变量值。"
        >
          <UiTextarea
            v-model="ai.templateInputJson.value"
            rows="6"
            maxlength="8000"
          />
        </UiFormField>
        <template #footer>
          <UiButton
            :disabled="ai.usingTemplate.value"
            @click="ai.templateDialogOpen.value = false"
            >取消</UiButton
          >
          <UiButton
            variant="primary"
            :disabled="ai.usingTemplate.value || !ai.selectedTemplateId.value"
            @click="ai.createFromTemplate"
          >
            {{ ai.usingTemplate.value ? '正在创建并请求模型…' : '创建并发送' }}
          </UiButton>
        </template>
      </UiDialog>
    </template>
  </FeishuOrdersPage>
</template>
