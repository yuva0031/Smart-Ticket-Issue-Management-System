import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { T } from '@angular/cdk/keycodes';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {

  constructor(private titleService: Title) { }

  protected readonly title = signal('Smart Ticket Management');
  ngOnInit() {
  this.titleService.setTitle("Smart Ticket Management");
}

}
