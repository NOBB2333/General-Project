<script setup lang="ts">
import { computed, reactive, ref, toRef } from 'vue';
import { AlertTriangle, KeyRound, RefreshCw, Search, ShieldCheck, UsersRound } from 'lucide-vue-next';
import {
  UiButton,
  UiCheckbox,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
} from '@/components/ui';
import type { TenantApi } from '@/api/core/tenant';
import TenantMenuTree from './components/TenantMenuTree.vue';
import { usePlatformTenantAuthorization } from './usePlatformTenantAuthorization';

const props = defineProps<{
  open: boolean;
  tenant: TenantApi.ListItem;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  saved: [];
}>();

const tenant = toRef(props, 'tenant');
const open = toRef(props, 'open');
const activeTab = ref('menus');
const page = reactive(usePlatformTenantAuthorization(tenant, open));
const dialogDescription = computed(
  () => `为“${props.tenant.name}”配置租户管理员、可见菜单和API限制。修改后刷新该租户会话生效。`,
);

async function submit() {
  if (await page.save()) {
    emit('saved');
    emit('update:open', false);
  }
}

function updateMenu(menuId: string, value: boolean | 'indeterminate') {
  page.toggleMenu(menuId, value === true);
}

function updateEndpoint(key: string, value: boolean | 'indeterminate') {
  page.toggleEndpointKey(key, value === true);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="`租户授权 · ${tenant.name}`"
    :description="dialogDescription"
    size="xl"
    @update:open="emit('update:open', $event)"
  >
    <div :class="$style.dialogContent">
      <p
        v-if="page.loadError"
        :class="$style.error"
        role="alert"
        aria-live="polite"
      >
        {{ page.loadError }}
        <UiButton
          size="sm"
          variant="secondary"
          :loading="page.isLoading"
          @click="page.load"
          >重试</UiButton
        >
      </p>
      <template v-else-if="page.dataReady && page.authorization">
        <section :class="$style.adminField">
          <div :class="$style.fieldHeading">
            <UsersRound
              :size="16"
              :stroke-width="1.8"
            />
            <strong>租户管理员</strong>
          </div>
          <div :class="$style.adminControls">
            <UiSelect
              v-model="page.selectedAdminUserId"
              label="租户管理员账号"
              :options="page.userOptions"
              :disabled="page.isUserLoading"
              placeholder="请选择租户管理员"
            />
            <label :class="$style.userSearch">
              <UiInput
                v-model="page.userKeyword"
                aria-label="搜索租户管理员"
                placeholder="按账号、姓名或邮箱搜索"
                @keydown.enter.prevent="page.searchUsers"
              />
              <UiButton
                size="sm"
                variant="secondary"
                :loading="page.isUserLoading"
                @click="page.searchUsers"
                >搜索</UiButton
              >
            </label>
          </div>
          <p
            v-if="page.userLoadError"
            :class="$style.inlineError"
            role="alert"
          >
            {{ page.userLoadError }}
            <button
              type="button"
              @click="page.searchUsers"
            >
              重试
            </button>
          </p>
          <div
            v-else
            :class="$style.userPager"
          >
            <span>匹配 {{ page.userTotalCount }} 个账号</span>
            <button
              type="button"
              :disabled="page.userPageIndex <= 0 || page.isUserLoading"
              @click="page.setUserPage(page.userPageIndex - 1)"
            >
              上一页
            </button>
            <span>{{ page.userPageIndex + 1 }} / {{ page.userPageCount }}</span>
            <button
              type="button"
              :disabled="page.userPageIndex + 1 >= page.userPageCount || page.isUserLoading"
              @click="page.setUserPage(page.userPageIndex + 1)"
            >
              下一页
            </button>
          </div>
        </section>

        <section :class="$style.remarkField">
          <label for="tenant-authorization-remark">授权备注</label>
          <UiInput
            id="tenant-authorization-remark"
            v-model="page.remark"
            maxlength="256"
            placeholder="可选"
          />
        </section>

        <UiTabs v-model="activeTab">
          <UiTabsList
            label="租户授权范围"
            :class="$style.tabs"
          >
            <UiTabsTrigger
              value="menus"
              :class="$style.tabTrigger"
            >
              <KeyRound
                :size="14"
                :stroke-width="1.8"
              />菜单与页面
              <span>{{ page.selectedMenuCount }}</span>
            </UiTabsTrigger>
            <UiTabsTrigger
              value="api"
              :class="$style.tabTrigger"
            >
              <ShieldCheck
                :size="14"
                :stroke-width="1.8"
              />API限制
              <span>{{ page.selectedApiBlacklist.size }}</span>
            </UiTabsTrigger>
          </UiTabsList>

          <UiTabPanel value="menus">
            <section :class="$style.permissionPanel">
              <header>
                <div>
                  <strong>可见菜单</strong>
                  <p>选择租户角色可访问的平台菜单。停用节点不可勾选。</p>
                </div>
                <UiButton
                  size="sm"
                  variant="secondary"
                  :loading="page.isLoading"
                  aria-label="刷新菜单与授权"
                  @click="page.load"
                >
                  <template #leading>
                    <RefreshCw
                      :size="13"
                      :stroke-width="1.8"
                    />
                  </template>
                  刷新
                </UiButton>
              </header>
              <p
                v-if="page.unknownMenuCount"
                :class="$style.notice"
                role="status"
              >
                <AlertTriangle
                  :size="14"
                  :stroke-width="1.8"
                />
                当前目录未返回的 {{ page.unknownMenuCount }} 个已授权菜单会在保存时保留。
              </p>
              <TenantMenuTree
                :items="page.menuTree"
                :selected-ids="page.selectedMenuIds"
                @toggle="updateMenu"
              />
            </section>
          </UiTabPanel>

          <UiTabPanel value="api">
            <section :class="$style.permissionPanel">
              <header>
                <div>
                  <strong>API 黑名单</strong>
                  <p>勾选会对该租户的请求进行限制；未勾选项目保持原有授权规则。</p>
                </div>
                <span :class="$style.endpointCount">{{ page.visibleEndpointCount }} 个端点</span>
              </header>
              <label :class="$style.searchField">
                <Search
                  :size="14"
                  :stroke-width="1.8"
                />
                <UiInput
                  v-model="page.endpointSearch"
                  aria-label="搜索API端点"
                  placeholder="搜索名称、请求路径或能力码"
                />
              </label>
              <p
                v-if="page.unknownEndpointKeys.length"
                :class="$style.notice"
                role="status"
              >
                <AlertTriangle
                  :size="14"
                  :stroke-width="1.8"
                />
                {{ page.unknownEndpointKeys.length }} 个目录外的历史限制会在保存时保留。
              </p>
              <div
                v-if="page.endpointOptionsReady"
                :class="$style.endpointGroups"
              >
                <article
                  v-for="group in page.filteredEndpointGroups"
                  :key="group.groupName"
                  :class="$style.endpointGroup"
                >
                  <h3>{{ group.groupName }}</h3>
                  <div
                    v-for="item in group.items"
                    :key="item.key"
                    :class="$style.endpointItem"
                  >
                    <UiCheckbox
                      :model-value="page.selectedApiBlacklist.has(item.key)"
                      :label="`限制 ${item.label} ${item.key}`"
                      @update:model-value="updateEndpoint(item.key, $event)"
                    />
                    <div :class="$style.endpointCopy">
                      <strong>{{ item.label }}</strong>
                      <code>{{ item.key }}</code>
                    </div>
                    <label
                      v-if="item.capabilityKey"
                      :class="$style.capabilityItem"
                    >
                      <UiCheckbox
                        :model-value="page.selectedApiBlacklist.has(item.capabilityKey)"
                        :label="`限制能力 ${item.capabilityKey}`"
                        @update:model-value="updateEndpoint(item.capabilityKey, $event)"
                      />
                      <span>能力限制：{{ item.capabilityKey }}</span>
                    </label>
                  </div>
                </article>
                <p
                  v-if="page.visibleEndpointCount === 0"
                  :class="$style.emptyText"
                >
                  没有匹配的API端点。
                </p>
              </div>
              <p
                v-else-if="!page.isLoading"
                :class="$style.inlineError"
                role="alert"
              >
                API目录未加载，无法安全覆盖现有限制。
              </p>
            </section>
          </UiTabPanel>
        </UiTabs>
      </template>
      <p
        v-else-if="page.isLoading"
        :class="$style.statusText"
        role="status"
        aria-live="polite"
      >
        正在读取租户授权、菜单与API目录…
      </p>

      <p
        v-if="page.saveError"
        :class="$style.error"
        role="alert"
        aria-live="polite"
      >
        {{ page.saveError }}
      </p>
    </div>
    <template #footer>
      <UiButton
        :disabled="page.isSaving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="page.isSaving"
        :disabled="!page.canSave"
        @click="submit"
        >保存授权</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
.dialogContent {
  @apply space-y-4;
}
.adminField,
.remarkField {
  @apply space-y-2 rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface);
}
.fieldHeading {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text);
}
.adminControls {
  @apply grid grid-cols-2 gap-3;
}
.userSearch {
  @apply flex min-w-0 items-center gap-2;
}
.userSearch > *:first-child {
  @apply min-w-0 flex-1;
}
.userPager {
  @apply flex items-center justify-end gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.userPager button,
.inlineError button {
  @apply rounded px-2 py-1;
  color: var(--app-primary);
}
.userPager button:hover:not(:disabled),
.inlineError button:hover {
  background: var(--ui-surface-hover);
}
.userPager button:disabled {
  @apply cursor-not-allowed opacity-50;
}
.remarkField label {
  @apply block text-[11px] font-medium;
  color: var(--ui-text-subtle);
}
.tabs {
  @apply flex w-full border-b;
  border-color: var(--ui-border);
}
.tabTrigger {
  @apply flex h-9 items-center gap-2 border-b-2 border-transparent px-3 text-[12px] font-medium;
  color: var(--ui-text-subtle);
}
.tabTrigger[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.tabTrigger span {
  @apply rounded px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-subtle);
}
.permissionPanel {
  @apply space-y-3 pt-3;
}
.permissionPanel > header {
  @apply flex items-center justify-between gap-3;
}
.permissionPanel > header strong {
  @apply text-[13px];
  color: var(--ui-text);
}
.permissionPanel > header p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-muted);
}
.searchField {
  @apply flex h-9 items-center gap-2 border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.searchField > *:last-child {
  @apply min-w-0 flex-1;
}
.endpointCount,
.notice {
  @apply flex items-center gap-1.5 text-[11px];
  color: var(--ui-text-subtle);
}
.notice {
  @apply rounded px-2.5 py-2;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.endpointGroups {
  @apply max-h-[380px] space-y-3 overflow-y-auto pr-1;
}
.endpointGroup {
  @apply overflow-hidden rounded border;
  border-color: var(--ui-border-subtle);
}
.endpointGroup h3 {
  @apply border-b px-3 py-2 text-[11px] font-semibold;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.endpointItem {
  @apply flex items-start gap-3 border-b px-3 py-2.5 text-[11px];
  border-color: var(--ui-border-subtle);
}
.endpointCopy {
  @apply min-w-0 flex-1;
}
.endpointCopy strong,
.endpointCopy code {
  @apply block break-all;
}
.endpointCopy strong {
  color: var(--ui-text);
}
.endpointCopy code {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.capabilityItem {
  @apply flex items-center gap-1.5 text-[10px];
  color: var(--ui-text-subtle);
}
.emptyText,
.statusText {
  @apply py-6 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.error,
.inlineError {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-danger);
}
@media (max-width: 720px) {
  .adminControls {
    @apply grid-cols-1;
  }
  .endpointItem {
    @apply flex-wrap;
  }
  .capabilityItem {
    @apply ml-7 w-full;
  }
}
</style>
