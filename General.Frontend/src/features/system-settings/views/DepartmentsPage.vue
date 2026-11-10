<script setup lang="ts">
import { departmentsModel } from '@/features/system-settings/model/departments.model';
import SystemSettingsListPage from './SystemSettingsListPage.vue';
</script>

<template>
  <SystemSettingsListPage :model="departmentsModel" />
</template>
