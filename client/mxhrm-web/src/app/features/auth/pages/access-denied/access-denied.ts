import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';

@Component({
  selector: 'app-access-denied',
  imports: [
    CommonModule,
    RouterLink,
    KENDO_BUTTONS
  ],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.scss'
})
export class AccessDenied {
  private readonly route = inject(ActivatedRoute);

  readonly requiredPermission = computed(() =>
    this.route.snapshot.queryParamMap.get('permission')
  );
}