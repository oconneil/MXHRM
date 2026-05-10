import { Component, OnDestroy, effect, inject } from '@angular/core';
import { ErrorService } from '../../services/error';

@Component({
  selector: 'app-global-error-alert',
  imports: [],
  templateUrl: './global-error-alert.html',
  styleUrl: './global-error-alert.scss',
})
export class GlobalErrorAlert implements OnDestroy {
  protected readonly errorService = inject(ErrorService);

  private autoDismissTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const error = this.errorService.lastError();

      this.clearAutoDismissTimer();

      if (error) {
        this.autoDismissTimer = setTimeout(() => {
          this.errorService.clear();
        }, 6000);
      }
    });
  }

  ngOnDestroy(): void {
    this.clearAutoDismissTimer();
  }

  protected close(): void {
    this.errorService.clear();
  }

  private clearAutoDismissTimer(): void {
    if (this.autoDismissTimer) {
      clearTimeout(this.autoDismissTimer);
      this.autoDismissTimer = null;
    }
  }
}
