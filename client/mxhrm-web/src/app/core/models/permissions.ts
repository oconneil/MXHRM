export const Permissions = {
  Employee: {
    Read: 'employee.read',
    Create: 'employee.create',
    Update: 'employee.update',
    Delete: 'employee.delete'
  },
  Role: {
    Manage: 'role.manage'
  },
  Audit: {
    Read: 'audit.read'
  },
  Activity: {
    Read: 'activity.read'
  }
} as const;