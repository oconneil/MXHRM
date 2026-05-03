import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeResponse, PagedResponse } from '../../models/employee';
import { EmployeeService } from '../../services/employee';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss'
})
export class EmployeeList implements OnInit {
  employees = signal<EmployeeResponse[]>([]);
  loading = signal(false);
  errorMessage = signal('');

  page = signal(1);
  pageSize = signal(10);
  totalItems = signal(0);
  totalPages = signal(0);
  search = signal('');

  constructor(private readonly employeeService: EmployeeService) { }

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.employeeService
      .getEmployees(this.page(), this.pageSize(), this.search())
      .subscribe({
        next: (res: PagedResponse<EmployeeResponse>) => {
          this.employees.set(res.items);
          this.totalItems.set(res.totalItems);
          this.totalPages.set(res.totalPages);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('ไม่สามารถโหลดข้อมูลพนักงานได้');
          this.loading.set(false);
        }
      });
  }

  onSearch(): void {
    this.page.set(1);
    this.loadEmployees();
  }

  previousPage(): void {
    if (this.page() <= 1) return;

    this.page.update(value => value - 1);
    this.loadEmployees();
  }

  nextPage(): void {
    if (this.page() >= this.totalPages()) return;

    this.page.update(value => value + 1);
    this.loadEmployees();
  }

  deleteEmployee(companyID: string, employeeID: string): void {
    const confirmed = confirm(`ต้องการลบพนักงาน ${employeeID} ใช่ไหม?`);

    if (!confirmed) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.employeeService.deleteEmployee(companyID, employeeID).subscribe({
      next: () => {
        this.loadEmployees();
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err?.error?.message ?? 'ไม่สามารถลบข้อมูลพนักงานได้'
        );
      }
    });
  }
}