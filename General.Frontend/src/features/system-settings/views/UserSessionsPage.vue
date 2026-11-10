<script setup lang="ts">
import { userSessionsModel } from '@/features/system-settings/model/user-sessions.model';
import SystemSettingsListPage from './SystemSettingsListPage.vue';
</script>

<template>
  <SystemSettingsListPage :model="userSessionsModel" />
</template>
