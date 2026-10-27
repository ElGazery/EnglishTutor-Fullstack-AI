import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {
    marked.use({
      gfm: true,
      breaks: true
    });
  }

  transform(value: string | null | undefined): SafeHtml {
    if (!value) return '';
    try {
      const parsed = marked.parse(value) as string;
      return this.sanitizer.bypassSecurityTrustHtml(parsed);
    } catch {
      return value;
    }
  }
}
