<script setup lang="ts">
import { ReferenceBoardPageSurface } from '@/components/page';
import { contactsModel } from '@/features/contacts/model/contacts.model';
</script>

<template>
  <ReferenceBoardPageSurface :model="contactsModel" />
</template>
