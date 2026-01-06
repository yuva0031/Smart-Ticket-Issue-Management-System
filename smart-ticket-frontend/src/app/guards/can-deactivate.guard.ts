import { CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';

export interface CanDeactivate {
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

export const canDeactivateGuard: CanDeactivateFn<CanDeactivate> = (
  component: CanDeactivate
) => {
  return component.canDeactivate ? component.canDeactivate() : true;
};