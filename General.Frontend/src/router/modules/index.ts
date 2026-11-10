import type { RouteRecordRaw } from 'vue-router';

import { admin_routes } from './admin.routes';
import { ai_routes } from './ai.routes';
import { attendance_routes } from './attendance.routes';
import { base_routes } from './base.routes';
import { business_routes } from './business.routes';
import { business_settings_routes } from './business-settings.routes';
import { company_calendar_routes } from './company-calendar.routes';
import { contacts_routes } from './contacts.routes';
import { crm_routes } from './crm.routes';
import { dashboard_routes } from './dashboard.routes';
import { drawings_routes } from './drawings.routes';
import { engineering_projects_routes } from './engineering-projects.routes';
import { finance_routes } from './finance.routes';
import { food_beverage_routes } from './food-beverage.routes';
import { hr_routes } from './hr.routes';
import { integration_routes } from './integration.routes';
import { inventory_routes } from './inventory.routes';
import { manufacturing_routes } from './manufacturing.routes';
import { outsourcing_routes } from './outsourcing.routes';
import { production_mgmt_routes } from './production-mgmt.routes';
import { projects_routes } from './projects.routes';
import { projects_new_routes } from './projects-new.routes';
import { promotion_routes } from './promotion.routes';
import { purchase_routes } from './purchase.routes';
import { sales_routes } from './sales.routes';
import { sales_performance_routes } from './sales-performance.routes';
import { service_routes } from './service.routes';
import { service_partners_routes } from './service-partners.routes';
import { shared_supply_routes } from './shared-supply.routes';
import { subscriptions_routes } from './subscriptions.routes';
import { supplier_portal_routes } from './supplier-portal.routes';
import { supplier_quotes_routes } from './supplier-quotes.routes';
import { system_init_routes } from './system-init.routes';
import { system_settings_routes } from './system-settings.routes';
import { todos_routes } from './todos.routes';
import { workflow_routes } from './workflow.routes';

export const featureRoutes: RouteRecordRaw[] = [
  ...admin_routes,
  ...ai_routes,
  ...attendance_routes,
  ...base_routes,
  ...business_routes,
  ...business_settings_routes,
  ...company_calendar_routes,
  ...contacts_routes,
  ...crm_routes,
  ...dashboard_routes,
  ...drawings_routes,
  ...engineering_projects_routes,
  ...finance_routes,
  ...food_beverage_routes,
  ...hr_routes,
  ...integration_routes,
  ...inventory_routes,
  ...manufacturing_routes,
  ...outsourcing_routes,
  ...production_mgmt_routes,
  ...projects_routes,
  ...projects_new_routes,
  ...promotion_routes,
  ...purchase_routes,
  ...sales_routes,
  ...sales_performance_routes,
  ...service_routes,
  ...service_partners_routes,
  ...shared_supply_routes,
  ...subscriptions_routes,
  ...supplier_portal_routes,
  ...supplier_quotes_routes,
  ...system_init_routes,
  ...system_settings_routes,
  ...todos_routes,
  ...workflow_routes,
];
