import type { Directive } from 'vue';
import { accessState } from '../session/access-store.ts';
import { hasRequiredAccessCodes } from '../navigation/page-registry.ts';

export type PermissionRequirement = string | readonly string[];

export function canAccess(required: PermissionRequirement, granted = accessState.codes): boolean {
  const requiredCodes = typeof required === 'string' ? [required] : required;
  return requiredCodes.length > 0 && hasRequiredAccessCodes(granted, requiredCodes);
}

function updateVisibility(element: HTMLElement, requirement: PermissionRequirement): void {
  element.hidden = !canAccess(requirement);
}

export const vAccess: Directive<HTMLElement, PermissionRequirement> = {
  mounted(element, binding) {
    updateVisibility(element, binding.value);
  },
  updated(element, binding) {
    updateVisibility(element, binding.value);
  },
};
