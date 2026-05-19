import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { DrawerItem, DrawerSelectEvent, KENDO_DRAWER } from '@progress/kendo-angular-layout';
import { KENDO_APPBAR } from '@progress/kendo-angular-navigation';
import {
  folderIcon,
  gearIcon,
  logoutIcon,
  menuIcon,
  userIcon
} from '@progress/kendo-svg-icons';
import { filter } from 'rxjs';
import { Permissions } from '../../models/permissions';
import { AuthService } from '../../services/auth';
import { GlobalErrorAlert } from '../../components/global-error-alert/global-error-alert';

type ShellDrawerItem = DrawerItem & {
  route?: string;
  permission?: string;
};

@Component({
  selector: 'app-main-layout',
  imports: [
    CommonModule,
    RouterOutlet,
    GlobalErrorAlert,
    KENDO_APPBAR,
    KENDO_BUTTONS,
    KENDO_DRAWER,
    KENDO_ICONS
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss'
})
export class MainLayout {
  private readonly router = inject(Router);
  protected readonly authService = inject(AuthService);

  protected readonly menuIcon = menuIcon;
  protected readonly logoutIcon = logoutIcon;
  protected readonly userIcon = userIcon;
  protected readonly expanded = signal(true);
  protected readonly currentUrl = signal(this.router.url);

  private readonly baseItems: ShellDrawerItem[] = [
    {
      text: 'Employees',
      route: '/employees',
      svgIcon: folderIcon,
      permission: Permissions.Employee.Read
    },
    {
      text: 'Roles',
      route: '/security-admin/roles',
      svgIcon: gearIcon,
      permission: Permissions.Role.Manage
    },
    {
      text: 'Users',
      route: '/security-admin/users',
      svgIcon: userIcon,
      permission: Permissions.Role.Manage
    },
    {
      text: 'Audit Logs',
      route: '/security-admin/audit-logs',
      svgIcon: folderIcon,
      permission: Permissions.Audit.Read
    },
    {
      text: 'Activity Logs',
      route: '/security-admin/user-activity-logs',
      svgIcon: folderIcon,
      permission: Permissions.Activity.Read
    }
  ];

  protected readonly drawerItems = computed(() =>
    this.baseItems
      .filter(item => !item.permission || this.authService.hasPermission(item.permission))
      .map(item => ({
        ...item,
        selected: this.isRouteActive(item.route)
      }))
  );

  constructor() {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(event => {
        this.currentUrl.set(event.urlAfterRedirects);
      });
  }

  protected toggleDrawer(): void {
    this.expanded.update(value => !value);
  }

  protected onDrawerSelect(event: DrawerSelectEvent): void {
    const item = event.item as ShellDrawerItem;

    if (item.route) {
      this.router.navigateByUrl(item.route);
    }
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private isRouteActive(route?: string): boolean {
    if (!route) {
      return false;
    }

    const currentUrl = this.currentUrl();
    return currentUrl === route || currentUrl.startsWith(`${route}/`);
  }
}
