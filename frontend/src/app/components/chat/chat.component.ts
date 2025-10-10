import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ChatService } from '../../services/chat.service';
import { ChatMessage, ChatMessageRequest } from '../../../types/chat-types';

@Component({
  selector: 'app-chat',
  imports: [FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css',
})
export class ChatComponent implements OnInit {
  userInput: string = '';
  userId: string = '';
  userName: string = '';
  chatMessages: ChatMessage[] = [];
  loading: boolean = false;

  private chatService = inject(ChatService);
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.userId = this.authService.getUserId();
    this.userName = this.authService.getUserName();
  }

  handleSubmit(e: MouseEvent) {
    if (!this.authService.isAdmin() && !this.authService.isUser())
      return console.error('Unauthorized');

    e.preventDefault();

    this.loading = true;

    const messageRequest: ChatMessageRequest = {
      userInput: this.userInput,
      userId: this.userId,
      userName: this.userName,
    };

    const userMessage: ChatMessage = {
      message: this.userInput,
      sender: 'User',
    };

    this.chatMessages.unshift(userMessage);

    this.userInput = '';

    this.chatService.postChatMessage(messageRequest).subscribe({
      next: (data) => {
        const AiMessage: ChatMessage = {
          message: data.body.response,
          sender: 'AI',
        };
        this.chatMessages.unshift(AiMessage);
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {
        this.loading = false;
      },
    });
  }
}
