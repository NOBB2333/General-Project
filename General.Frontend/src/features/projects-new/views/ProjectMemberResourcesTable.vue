<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { updateProjectMemberRolesApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiTextarea } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';

const props = defineProps<{ detail: ProjectApi.Detail }>();
const emit = defineEmits<{ changed: [] }>();

const canManageMemberRoles = computed(() => hasAccessCode('Project.Member.Manage'));
const editingMemberId = ref<string | null>(null);
const savingMemberId = ref('');
const roleDrafts = reactive<Record<string, string>>({});
const roleErrors = reactive<Record<string, string>>({});
const roleNotice = ref('');

const memberRows = computed(() =>
  props.detail.members.map((member) => {
    const assignedTaskCount = props.detail.tasks.filter((task) => taskParticipantIds(task).has(member.userId)).length;
    const registeredWorklogHours = props.detail.worklogs
      .filter((worklog) => worklog.userId === member.userId)
      .reduce((total, worklog) => total + worklog.hours, 0);

    return { assignedTaskCount, member, registeredWorklogHours };
  }),
);

function taskParticipantIds(task: ProjectApi.TaskItem): Set<string> {
  return new Set(
    [
      task.ownerUserId,
      task.productOwnerUserId,
      task.developerOwnerUserId,
      task.testerOwnerUserId,
      ...task.productOwnerUserIds,
      ...task.developerOwnerUserIds,
      ...task.testerOwnerUserIds,
    ].filter((userId): userId is string => Boolean(userId)),
  );
}

function formatBackendDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}

function displayMemberName(member: ProjectApi.DetailMember): string {
  const userName = member.userName.trim();
  return userName && userName !== '-' && userName !== '—' ? userName : '未提供姓名';
}

function displayOrganizationName(value: null | string): string {
  const organizationName = value?.trim();
  return organizationName && organizationName !== '-' && organizationName !== '—' ? organizationName : '未提供组织';
}

function formatHours(value: number): string {
  return new Intl.NumberFormat('zh-CN', { maximumFractionDigits: 2 }).format(value);
}

function beginRoleEdit(member: ProjectApi.DetailMember) {
  roleDrafts[member.id] = member.roleNames.join('\n');
  delete roleErrors[member.id];
  roleNotice.value = '';
  editingMemberId.value = member.id;
}

function cancelRoleEdit(memberId: string) {
  editingMemberId.value = null;
  delete roleDrafts[memberId];
  delete roleErrors[memberId];
}

function roleNamesFor(memberId: string): string[] {
  return (roleDrafts[memberId] || '')
    .split(/\r?\n/)
    .map((roleName) => roleName.trim())
    .filter(Boolean);
}

async function saveRoleEdit(memberId: string) {
  const roleNames = roleNamesFor(memberId);
  if (roleNames.length === 0) {
    roleErrors[memberId] = '项目成员至少需要一个角色。';
    return;
  }

  savingMemberId.value = memberId;
  roleErrors[memberId] = '';
  roleNotice.value = '';
  try {
    const saved = await updateProjectMemberRolesApi(memberId, { roleNames });
    if (!saved) {
      roleErrors[memberId] = '服务端没有确认角色更新，请检查后重试。';
      return;
    }

    editingMemberId.value = null;
    delete roleDrafts[memberId];
    roleNotice.value = '角色已保存，正在重新读取项目成员。';
    emit('changed');
  } catch (error) {
    roleErrors[memberId] = error instanceof Error && error.message ? error.message : '角色更新失败，请稍后重试。';
  } finally {
    savingMemberId.value = '';
  }
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-labelledby="project-members-title"
  >
    <header :class="$style.header">
      <div>
        <h3
          id="project-members-title"
          :class="$style.heading"
        >
          项目成员与资源投入
        </h3>
        <p :class="$style.description">
          成员、关联任务及工时按当前用户可见的数据展示。系统未提供人力容量/可用率契约，因此不计算负载率。
        </p>
      </div>
      <span :class="$style.count">{{ memberRows.length }} 位可见成员</span>
    </header>

    <p
      v-if="roleNotice"
      :class="$style.notice"
      role="status"
      aria-live="polite"
    >
      {{ roleNotice }}
    </p>

    <div
      v-if="memberRows.length"
      :class="$style.tableWrap"
      role="region"
      aria-label="项目成员和可见资源投入"
      tabindex="0"
    >
      <table :class="$style.table">
        <caption :class="$style.srOnly">
          项目成员、角色及当前可见的任务和工时投入
        </caption>
        <thead>
          <tr>
            <th scope="col">成员</th>
            <th scope="col">组织</th>
            <th scope="col">项目角色</th>
            <th scope="col">成员状态</th>
            <th scope="col">加入 / 离开</th>
            <th scope="col">关联任务</th>
            <th scope="col">已登记工时</th>
            <th scope="col">操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in memberRows"
            :key="row.member.id"
          >
            <td>
              <strong>{{ displayMemberName(row.member) }}</strong>
              <span :class="$style.accessLevel">{{ row.member.accessLevel }}</span>
            </td>
            <td>{{ displayOrganizationName(row.member.organizationUnitName) }}</td>
            <td>
              <template v-if="editingMemberId === row.member.id">
                <label
                  :for="`project-member-roles-${row.member.id}`"
                  :class="$style.srOnly"
                >
                  {{ displayMemberName(row.member) }}的项目角色，每行一个
                </label>
                <UiTextarea
                  :id="`project-member-roles-${row.member.id}`"
                  v-model="roleDrafts[row.member.id]"
                  :class="$style.roleInput"
                  :disabled="savingMemberId === row.member.id"
                  rows="3"
                  aria-describedby="project-role-hint"
                />
                <span
                  v-if="roleErrors[row.member.id]"
                  :class="$style.error"
                  role="alert"
                >
                  {{ roleErrors[row.member.id] }}
                </span>
                <div :class="$style.rowActions">
                  <UiButton
                    variant="primary"
                    size="sm"
                    :disabled="savingMemberId === row.member.id"
                    @click="saveRoleEdit(row.member.id)"
                  >
                    {{ savingMemberId === row.member.id ? '保存中…' : '保存角色' }}
                  </UiButton>
                  <UiButton
                    variant="ghost"
                    size="sm"
                    :disabled="savingMemberId === row.member.id"
                    @click="cancelRoleEdit(row.member.id)"
                  >
                    取消
                  </UiButton>
                </div>
              </template>
              <template v-else>
                <div :class="$style.roles">
                  <span
                    v-for="roleName in row.member.roleNames"
                    :key="roleName"
                    :class="$style.roleTag"
                  >
                    {{ roleName }}
                  </span>
                  <span v-if="row.member.roleNames.length === 0">未设置</span>
                </div>
              </template>
            </td>
            <td>{{ row.member.isActive ? '在岗' : '已离开' }}</td>
            <td>{{ formatBackendDate(row.member.joinDate) }} / {{ formatBackendDate(row.member.leaveDate) }}</td>
            <td>{{ row.assignedTaskCount }} 项</td>
            <td>{{ formatHours(row.registeredWorklogHours) }} 小时</td>
            <td>
              <UiButton
                v-if="canManageMemberRoles && row.member.isActive && editingMemberId !== row.member.id"
                variant="ghost"
                size="sm"
                :aria-label="`编辑${displayMemberName(row.member)}的项目角色`"
                @click="beginRoleEdit(row.member)"
              >
                编辑角色
              </UiButton>
              <span v-else-if="editingMemberId !== row.member.id">—</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <p
      v-else
      :class="$style.empty"
    >
      当前用户可见范围内没有项目成员。
    </p>
    <p
      id="project-role-hint"
      :class="$style.footnote"
    >
      每行填写一个项目角色。项目角色只维护项目内分工；平台角色和权限由平台角色管理维护。关联任务数按可见任务角色去重，工时只汇总当前详情接口返回的记录。
    </p>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply space-y-4 rounded-2xl border bg-white p-4;
  border-color: var(--ui-border);
}

.header {
  @apply flex flex-wrap items-start justify-between gap-3;
}

.heading {
  @apply text-base font-semibold;
  color: var(--ui-text);
}

.description,
.footnote {
  @apply mt-1 text-sm;
  color: var(--ui-text-muted);
}

.count {
  @apply rounded-full px-3 py-1 text-sm;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.tableWrap {
  @apply overflow-x-auto rounded-xl border;
  border-color: var(--ui-border);
}

.table {
  @apply w-full border-collapse text-left text-sm;
  min-width: 68rem;
}

.table th {
  @apply px-3 py-2 font-medium;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.table td {
  @apply px-3 py-3 align-top;
  border-top: 1px solid var(--ui-border-subtle);
}

.accessLevel {
  @apply mt-1 block text-xs;
  color: var(--ui-text-subtle);
}

.roles {
  @apply flex flex-wrap gap-1;
}

.roleTag {
  @apply rounded-full border px-2 py-0.5 text-xs;
  border-color: var(--ui-border);
  color: var(--ui-text);
}

.roleInput {
  min-width: 12rem;
}

.rowActions {
  @apply mt-2 flex flex-wrap gap-2;
}

.error {
  @apply mt-1 block text-sm;
  color: var(--ui-danger);
}

.notice {
  @apply rounded-lg border px-3 py-2 text-sm;
  border-color: var(--ui-border);
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}

.empty {
  @apply rounded-xl border border-dashed px-4 py-8 text-center text-sm;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}

.srOnly {
  @apply sr-only;
}
</style>
