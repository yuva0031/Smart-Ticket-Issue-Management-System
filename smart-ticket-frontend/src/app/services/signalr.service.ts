import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {

  private hubConnection!: signalR.HubConnection;

  private initialized = false;
  private listenersRegistered = false;

  private connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected
  );

  private ticketUpdated$ = new BehaviorSubject<any | null>(null);
  private commentAdded$ = new BehaviorSubject<any | null>(null);
  private accountActivated$ = new BehaviorSubject<any | null>(null);
  private userRegistered$ = new BehaviorSubject<any | null>(null);
  private userApproved$ = new BehaviorSubject<any | null>(null); 

  constructor() {}

  private initialize(): void {
    if (this.initialized) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7074/ticketHub', {
        accessTokenFactory: () => localStorage.getItem('token') || '',
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.connectionState$.next(signalR.HubConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.connectionState$.next(signalR.HubConnectionState.Connected);
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR disconnected');
      this.connectionState$.next(signalR.HubConnectionState.Disconnected);
      this.listenersRegistered = false; 
    });

    this.initialized = true;
  }

  async startConnection(): Promise<void> {
    this.initialize();

    if (this.hubConnection.state === signalR.HubConnectionState.Connected)
      return;

    await this.hubConnection.start();
    this.connectionState$.next(signalR.HubConnectionState.Connected);

    this.registerListeners();
    console.log('SignalR connected:', this.hubConnection.connectionId);
  }

  private registerListeners(): void {
    if (this.listenersRegistered) return;

    console.log('📡 Registering SignalR listeners');

    this.hubConnection.on('TicketUpdated', data => {
      console.log('TicketUpdated:', data);
      this.ticketUpdated$.next(data);
    });

    this.hubConnection.on('CommentAdded', data => {
      console.log('CommentAdded:', data);
      this.commentAdded$.next(data);
    });

    this.hubConnection.on('AccountActivated', data => {
      console.log('AccountActivated:', data);
      this.accountActivated$.next(data);
    });

    this.hubConnection.on('UserRegistered', data => {
      console.log('UserRegistered:', data);
      this.userRegistered$.next(data);
    });

    this.hubConnection.on('UserApproved', data => {
      console.log('UserApproved:', data);
      this.userApproved$.next(data);
    });

    this.listenersRegistered = true;
  }

  getTicketUpdated$(): Observable<any | null> {
    return this.ticketUpdated$.asObservable();
  }

  getCommentAdded$(): Observable<any | null> {
    return this.commentAdded$.asObservable();
  }

  getAccountActivated$(): Observable<any | null> {
    return this.accountActivated$.asObservable();
  }

  getUserRegistered$(): Observable<any | null> {
    return this.userRegistered$.asObservable();
  }

  getUserApproved$(): Observable<any | null> {
    return this.userApproved$.asObservable();
  }

  getConnectionState$(): Observable<signalR.HubConnectionState> {
    return this.connectionState$.asObservable();
  }

  invoke(method: string, ...args: any[]): Promise<any> {
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return Promise.reject('SignalR not connected');
    }
    return this.hubConnection.invoke(method, ...args);
  }
}
