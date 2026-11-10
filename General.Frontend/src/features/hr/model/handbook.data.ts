import { defineReferenceFixture } from '@/components/page';
import type { HandbookChapter } from './handbook.types';

export const handbookChapters: readonly HandbookChapter[] = [
  {
    id: 'company',
    number: 1,
    title: '公司概况',
    updatedAt: '2026-01-01',
    tone: 'blue',
    clauses: [
      {
        id: 'company-introduction',
        number: '1.1',
        title: '公司简介',
        content:
          '本公司成立于2010年，专注于工业自动化控制系统的设计、集成、安装调试及售后服务。总部位于上海市浦东新区，业务覆盖电力、石化、冶金等行业。公司秉承“科技驱动、品质至上”的理念，致力于为客户提供一站式自动化解决方案。',
      },
      {
        id: 'organization',
        number: '1.2',
        title: '组织架构',
        content:
          '公司设有工程部、销售部、技术研发部、财务部、人事行政部和市场部。各部门依照岗位职责协同完成项目交付、客户服务、产品研发和经营管理工作。',
      },
      {
        id: 'culture',
        number: '1.3',
        title: '企业文化与价值观',
        content:
          '核心价值观：专业、诚信、创新、协作。企业使命：用自动化技术赋能工业升级。企业愿景：成为国内领先的工业自动化系统集成商。',
      },
    ],
  },
  {
    id: 'onboarding',
    number: 2,
    title: '入职指南',
    updatedAt: '2026-01-08',
    tone: 'green',
    clauses: [
      {
        id: 'onboarding-process',
        number: '2.1',
        title: '入职流程',
        content: '新员工应按通知时间报到，依次完成人事资料核验、劳动合同签署、办公资源领取、部门报到和岗位培训。',
      },
      {
        id: 'probation',
        number: '2.2',
        title: '试用期管理',
        content: '试用期内由直属负责人设定工作目标并进行阶段反馈。转正前完成人岗匹配评估，评估结果作为转正审批依据。',
        important: true,
      },
      {
        id: 'mentor',
        number: '2.3',
        title: '导师制度',
        content: '公司为新员工安排岗位导师，导师负责业务辅导、流程答疑和团队融入，并记录辅导进展。',
      },
    ],
  },
  {
    id: 'attendance',
    number: 3,
    title: '考勤与假期',
    updatedAt: '2026-01-12',
    tone: 'purple',
    clauses: [
      {
        id: 'working-hours',
        number: '3.1',
        title: '工作时间',
        content: '公司实行标准工时制度，员工应按规定完成签到、签退和外勤登记，异常考勤需及时提交说明。',
      },
      {
        id: 'leave',
        number: '3.2',
        title: '假期制度',
        content: '员工可依法享受年假、婚假、产假、陪产假等假期。请假应提前提交申请并完成工作交接。',
        important: true,
      },
    ],
  },
  {
    id: 'compensation',
    number: 4,
    title: '薪酬福利',
    updatedAt: '2026-01-15',
    tone: 'orange',
    clauses: [
      {
        id: 'salary',
        number: '4.1',
        title: '薪酬结构',
        content: '员工薪酬由基本工资、岗位津贴、绩效奖金及相关补贴组成，工资按月发放。',
      },
      {
        id: 'insurance',
        number: '4.2',
        title: '社保公积金',
        content: '公司依照国家及属地政策为员工缴纳社会保险和住房公积金。',
      },
      {
        id: 'benefits',
        number: '4.3',
        title: '员工福利',
        content: '公司结合经营情况提供节日礼品、年度体检、团队活动和员工关怀等福利。',
      },
    ],
  },
  {
    id: 'training',
    number: 5,
    title: '培训发展',
    updatedAt: '2026-01-18',
    tone: 'cyan',
    clauses: [
      {
        id: 'training-system',
        number: '5.1',
        title: '培训体系',
        content: '培训分为入职培训、岗位技能培训、管理培训和专业认证，学习记录纳入员工档案。',
      },
      {
        id: 'promotion',
        number: '5.2',
        title: '职级晋升',
        content: '公司依据岗位能力、绩效表现和价值观行为进行职级评审，为员工提供专业和管理双通道发展路径。',
      },
    ],
  },
  {
    id: 'conduct',
    number: 6,
    title: '行为规范',
    updatedAt: '2026-01-20',
    tone: 'blue',
    clauses: [
      {
        id: 'ethics',
        number: '6.1',
        title: '职业道德',
        content: '员工应诚实守信、恪尽职守，避免利益冲突，不得利用职务便利谋取不当利益。',
      },
      {
        id: 'dress',
        number: '6.2',
        title: '着装规范',
        content: '工作期间保持整洁得体；进入客户现场和生产区域时应遵守对应着装与防护要求。',
      },
      {
        id: 'confidentiality',
        number: '6.3',
        title: '保密制度',
        content: '员工应妥善保护客户资料、技术文档、商业数据和个人信息，未经授权不得对外披露。',
        important: true,
      },
    ],
  },
  {
    id: 'safety',
    number: 7,
    title: '安全生产',
    updatedAt: '2026-01-25',
    tone: 'orange',
    clauses: [
      {
        id: 'safety-principles',
        number: '7.1',
        title: '安全总则',
        content: '安全生产人人有责。员工应参加安全培训，识别作业风险，发现隐患后立即报告。',
        important: true,
      },
      {
        id: 'site-safety',
        number: '7.2',
        title: '现场安全规范',
        content: '进入项目现场必须佩戴规定的劳动防护用品，严格执行作业许可、设备隔离和应急处置要求。',
        important: true,
      },
    ],
  },
];

export const handbookFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/hr/handbook',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  rows: handbookChapters.flatMap((chapter) => chapter.clauses),
  defaults: { version: 'V3.2', publishedAt: '2026-02-01', editor: '孙丽萍', readRate: 74 },
});
