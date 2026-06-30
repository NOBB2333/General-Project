export const taskColumns = [
  { field: 'taskCode', slots: { default: 'taskCode' }, title: '需求编号', width: 120 },
  { align: 'left' as const, field: 'title', slots: { default: 'title' }, title: '需求标题', width: 240 },
  { align: 'left' as const, field: 'description', slots: { default: 'description' }, title: '需求说明', width: 360 },
  { align: 'left' as const, field: 'wbsNodeName', slots: { default: 'wbsNodeName' }, title: '所属 WBS', width: 200 },
  { align: 'left' as const, field: 'sprintName', slots: { default: 'sprintName' }, title: '所属排期', width: 160 },
  { field: 'ownerName', slots: { default: 'ownerName' }, title: '负责人', width: 100 },
  { field: 'productOwnerName', slots: { default: 'productOwnerName' }, title: '产品', width: 150 },
  { field: 'developerOwnerName', slots: { default: 'developerOwnerName' }, title: '开发', width: 150 },
  { field: 'testerOwnerName', slots: { default: 'testerOwnerName' }, title: '测试', width: 150 },
  { field: 'plannedStartTime', slots: { default: 'plannedStartTime' }, title: '计划开始', width: 120 },
  { field: 'plannedEndTime', slots: { default: 'plannedEndTime' }, title: '计划完成', width: 120 },
  { field: 'estimatedWorkHours', slots: { default: 'estimatedWorkHours' }, title: '预计工时', width: 100 },
  { field: 'actualWorkHours', slots: { default: 'actualWorkHours' }, title: '已填报工时', width: 110 },
  { field: 'priority', slots: { default: 'priority' }, title: '优先级', width: 90 },
  { field: 'isOutOfContract', slots: { default: 'isOutOfContract' }, title: '合同外需求', width: 110 },
  { field: 'prd', slots: { default: 'prd' }, title: 'PRD', width: 140 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 100 },
  { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 130 },
];

export const issueColumns = [
  { align: 'left' as const, field: 'title', slots: { default: 'title' }, title: '问题项', width: 240 },
  { align: 'left' as const, field: 'requirementTitle', slots: { default: 'requirementTitle' }, title: '需求', width: 160 },
  { field: 'productOwnerName', slots: { default: 'productOwnerName' }, title: '产品', width: 100 },
  { field: 'developerOwnerName', slots: { default: 'developerOwnerName' }, title: '开发', width: 100 },
  { field: 'testerOwnerName', slots: { default: 'testerOwnerName' }, title: '测试', width: 100 },
  { field: 'type', title: '类型', width: 90 },
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 80 },
  { field: 'ownerName', title: '负责人', width: 100 },
  { field: 'dueDate', slots: { default: 'dueDate' }, title: '到期', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 100 },
  { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 120 },
];

export const documentColumns = [
  { field: 'category', slots: { default: 'category' }, title: '分类', width: 100 },
  { align: 'left' as const, field: 'title', slots: { default: 'title' }, title: '文档标题', minWidth: 220 },
  { field: 'version', title: '版本', width: 100 },
  { field: 'ownerName', title: '负责人', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
];

export const memberColumns = [
  { field: 'userName', title: '成员', width: 120 },
  { field: 'organizationUnitName', title: '所属部门', width: 140 },
  { field: 'roleNames', slots: { default: 'roleNames' }, title: '项目角色', width: 200 },
  { field: 'taskCount', slots: { default: 'taskCount' }, title: '关联需求', width: 110 },
  { field: 'estimatedWorkHours', slots: { default: 'estimatedWorkHours' }, title: '预计工时', width: 110 },
  { field: 'worklogHours', slots: { default: 'worklogHours' }, title: '累计工时', width: 110 },
  { field: 'loadHint', slots: { default: 'loadHint' }, title: '投入提示', width: 130 },
  { field: 'accessLevel', title: '访问级别', width: 110 },
  { field: 'joinDate', slots: { default: 'joinDate' }, title: '加入时间', width: 130 },
  { field: 'actions', fixed: 'right' as const, slots: { default: 'actions' }, title: '操作', width: 140 },
];

export const worklogColumns = [
  { field: 'workDate', slots: { default: 'workDate' }, title: '日期', width: 140 },
  { field: 'userName', title: '成员', width: 120 },
  { align: 'left' as const, field: 'taskTitle', title: '关联事项', minWidth: 180 },
  { align: 'left' as const, field: 'summary', title: '工作内容', minWidth: 220 },
  { field: 'hours', title: '工时', width: 90 },
];
