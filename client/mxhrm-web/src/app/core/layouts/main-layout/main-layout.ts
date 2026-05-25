import { CommonModule } from '@angular/common';
import {
  Component,
  HostListener,
  OnDestroy,
  OnInit,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { DrawerItem, DrawerSelectEvent, KENDO_DRAWER } from '@progress/kendo-angular-layout';
import { KENDO_APPBAR } from '@progress/kendo-angular-navigation';
import { Align, Collision, KENDO_POPUP, Margin } from '@progress/kendo-angular-popup';
import {
  arrowRotateCwIcon,
  bellIcon,
  checkIcon,
  exclamationCircleIcon,
  fileReportIcon,
  folderIcon,
  gearIcon,
  logoutIcon,
  menuIcon,
  userIcon,
  xIcon
} from '@progress/kendo-svg-icons';
import { filter } from 'rxjs';
import { Permissions } from '../../models/permissions';
import { AuthService } from '../../services/auth';
import { GlobalErrorAlert } from '../../components/global-error-alert/global-error-alert';
import { NotificationService } from '../../notifications/notification-service';
import { RealtimeService } from '../../realtime/realtime';
import { NotificationItem } from '../../notifications/notification';

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
    KENDO_ICONS,
    KENDO_POPUP,
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  protected readonly authService = inject(AuthService);
  protected readonly notificationService = inject(NotificationService);
  private readonly realtimeService = inject(RealtimeService);

  protected readonly menuIcon = menuIcon;
  protected readonly logoutIcon = logoutIcon;
  protected readonly userIcon = userIcon;
  protected readonly bellIcon = bellIcon;
  protected readonly checkIcon = checkIcon;
  protected readonly fileReportIcon = fileReportIcon;
  protected readonly xIcon = xIcon;
  protected readonly refreshIcon = arrowRotateCwIcon;
  protected readonly errorIcon = exclamationCircleIcon;

  protected readonly expanded = signal(true);
  protected readonly notificationPanelOpen = signal(false);
  protected readonly realtimeToast = signal<NotificationItem | null>(null);
  protected readonly currentUrl = signal(this.router.url);

  protected readonly notificationSkeletonRows = [1, 2, 3];

  private toastTimeout?: ReturnType<typeof setTimeout>;

  protected readonly realtimeConnected = computed(() => this.realtimeService.connected());
  protected readonly primaryRole = computed(
    () => this.authService.currentUser()?.roles?.[0] ?? 'Member',
  );

  protected readonly notificationAnchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  protected readonly notificationPopupAlign: Align = { horizontal: 'right', vertical: 'top' };
  protected readonly notificationCollision: Collision = { horizontal: 'fit', vertical: 'flip' };
  protected readonly notificationMargin: Margin = { horizontal: 0, vertical: 12 };

  private readonly baseItems: ShellDrawerItem[] = [
    {
      text: 'Employees',
      route: '/employees',
      svgIcon: folderIcon,
      permission: Permissions.Employee.Read,
    },
    {
      text: 'Roles',
      route: '/security-admin/roles',
      svgIcon: gearIcon,
      permission: Permissions.Role.Manage,
    },
    {
      text: 'Users',
      route: '/security-admin/users',
      svgIcon: userIcon,
      permission: Permissions.Role.Manage,
    },
    {
      text: 'Audit Logs',
      route: '/security-admin/audit-logs',
      svgIcon: folderIcon,
      permission: Permissions.Audit.Read,
    },
    {
      text: 'Activity Logs',
      route: '/security-admin/user-activity-logs',
      svgIcon: folderIcon,
      permission: Permissions.Activity.Read,
    },
    {
      text: 'Reports',
      route: '/reports/employee-summary',
      svgIcon: folderIcon,
      permission: Permissions.Employee.Read,
    },
    {
      text: 'Audit Report',
      route: '/reports/audit',
      svgIcon: folderIcon,
      permission: Permissions.Audit.Read,
    },
    {
      text: 'Generated Reports',
      route: '/reports/generated',
      svgIcon: folderIcon,
      permission: Permissions.Report.Manage,
    },
  ];

  protected readonly drawerItems = computed(() =>
    this.baseItems
      .filter((item) => !item.permission || this.authService.hasPermission(item.permission))
      .map((item) => ({
        ...item,
        selected: this.isRouteActive(item.route),
      })),
  );

  constructor() {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.currentUrl.set(event.urlAfterRedirects);
      });

    effect(() => {
      const notification = this.notificationService.latestNotification();

      if (!notification) {
        return;
      }

      this.showRealtimeToast(notification);
    });
  }

  ngOnInit(): void {
    this.notificationService.load();
    this.realtimeService.start();
  }

  ngOnDestroy(): void {
    this.clearToastTimeout();
    this.realtimeService.stop();
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (!this.notificationPanelOpen()) {
      return;
    }

    const target = event.target;

    if (!(target instanceof Element)) {
      return;
    }

    const clickedInsideNotification =
      target.closest('.notification-trigger') ||
      target.closest('.notification-panel');

    if (!clickedInsideNotification) {
      this.closeNotifications();
    }
  }

  protected toggleDrawer(): void {
    this.expanded.update((value) => !value);
  }

  protected onDrawerSelect(event: DrawerSelectEvent): void {
    const item = event.item as ShellDrawerItem;

    if (item.route) {
      this.router.navigateByUrl(item.route);
    }
  }

  protected toggleNotifications(): void {
    this.notificationPanelOpen.update((value) => !value);
  }

  protected closeNotifications(): void {
    this.notificationPanelOpen.set(false);
  }
  
  protected refreshNotifications(): void {
    this.notificationService.load();
  }

  protected dismissRealtimeToast(): void {
    this.realtimeToast.set(null);
    this.clearToastTimeout();
  }

  protected openToastNotification(): void {
    const notification = this.realtimeToast();

    if (!notification) {
      return;
    }

    this.dismissRealtimeToast();
    this.openNotification(notification);
  }

  private showRealtimeToast(notification: NotificationItem): void {
    this.clearToastTimeout();
    this.realtimeToast.set(notification);

    this.toastTimeout = setTimeout(() => {
      this.realtimeToast.set(null);
    }, 5000);
  }

  private clearToastTimeout(): void {
    if (!this.toastTimeout) {
      return;
    }

    clearTimeout(this.toastTimeout);
    this.toastTimeout = undefined;
  }

  protected openNotification(notification: NotificationItem): void {
    this.notificationService.markAsRead(notification.id);
    this.notificationPanelOpen.set(false);

    if (notification.route) {
      this.router.navigateByUrl(notification.route);
    }
  }

  protected markAllNotificationsAsRead(): void {
    this.notificationService.markAllAsRead();
  }

  protected logout(): void {
    this.realtimeService.stop();
    this.notificationService.clear();
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
