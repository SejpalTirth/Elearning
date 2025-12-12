import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'moduleTitle',
  standalone: true
})
export class ModuleTitlePipe implements PipeTransform {
  transform(modules: any[], id: number | null): string {
    if (!modules || !id) {return '';}
    const found = modules.find(m => m.id === id);
    return found ? found.title : '';
  }
}
