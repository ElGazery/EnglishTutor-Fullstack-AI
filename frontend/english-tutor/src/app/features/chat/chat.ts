import {
  Component, OnInit, OnDestroy, ElementRef, ViewChild,
  AfterViewChecked, signal, ChangeDetectorRef, inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../../core/services/chat.service';
import { VoiceService, VoiceState } from '../../core/services/voice.service';
import { AnswerMode, ChatMessage, MessageRole, SessionSummary } from '../../core/models/chat.model';
import { MarkdownPipe } from '../../shared/pipes/markdown.pipe';

export interface PromptTrack {
  icon: string;
  tag: string;
  title: string;
  description: string;
  examplePrompt: string;
  mode: AnswerMode;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MarkdownPipe],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class ChatComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;
  @ViewChild('messageInput') private messageInputRef!: ElementRef<HTMLTextAreaElement>;

  public authService = inject(AuthService);
  public voiceService = inject(VoiceService);
  private chatService = inject(ChatService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  // Sessions reactive state (Instant UI updates without page refresh)
  sessions = signal<SessionSummary[]>([]);
  activeSessionId: number | null = null;
  activeSessionTitle = 'New Lesson';

  // Messages reactive state
  messages = signal<ChatMessage[]>([]);

  // Composer state
  inputText = '';
  selectedMode: AnswerMode = AnswerMode.Detailed;
  AnswerModeEnum = AnswerMode;
  MessageRoleEnum = MessageRole;

  // Status flags
  isLoadingSessions = false;
  isLoadingMessages = false;
  isSending = false;
  errorMessage = '';
  isSidebarOpen = false;
  copiedMessageId: number | null = null;
  typingMessageId: number | null = null;

  private typewriterInterval: any = null;
  private currentFullText = '';
  private shouldScrollToBottom = false;

  // Educational Learning Tracks for Preparatory Students
  learningTracks: PromptTrack[] = [
    {
      icon: '📘',
      tag: 'Grammar Focus',
      title: 'Tenses & Rules',
      description: 'Understand past, present, and future forms with clear bilingual examples.',
      examplePrompt: "Explain the difference between 'Past Simple' and 'Past Continuous' with examples.",
      mode: AnswerMode.Detailed
    },
    {
      icon: '✍️',
      tag: 'Accuracy',
      title: 'Sentence Correction',
      description: 'Paste any English sentence to detect and fix mistakes instantly.',
      examplePrompt: "Correct this sentence and tell me why: 'He don't likes playing football yesterday.'",
      mode: AnswerMode.Quick
    },
    {
      icon: '🗣️',
      tag: 'Speaking Practice',
      title: 'Everyday Dialogues',
      description: 'Practice speaking phrases used in school, travel, and conversations.',
      examplePrompt: "Give me 5 useful English phrases for introducing myself to a new classmate.",
      mode: AnswerMode.Detailed
    },
    {
      icon: '💡',
      tag: 'Vocabulary',
      title: 'Word Meanings & Usage',
      description: 'Learn new vocabulary words, synonyms, and natural sentence usage.',
      examplePrompt: "What does 'remarkable' mean, and can you use it in 2 simple sentences?",
      mode: AnswerMode.Detailed
    }
  ];

  ngOnInit(): void {
    this.loadSessions();
  }

  ngOnDestroy(): void {
    this.cancelTypewriter();
    this.voiceService.stopSpeaking();
    this.voiceService.stopListening();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  loadSessions(): void {
    this.isLoadingSessions = true;
    this.chatService.getMySessions().subscribe({
      next: (res) => {
        this.isLoadingSessions = false;
        if (res.isSuccess && res.data) {
          this.sessions.set(res.data);
          if (this.sessions().length > 0 && !this.activeSessionId) {
            this.selectSession(this.sessions()[0].id);
          }
        }
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingSessions = false;
        this.cdr.detectChanges();
      }
    });
  }

  selectSession(sessionId: number): void {
    this.cancelTypewriter();
    this.voiceService.stopSpeaking();

    if (this.activeSessionId === sessionId && this.messages().length > 0) return;

    this.activeSessionId = sessionId;
    const session = this.sessions().find(s => s.id === sessionId);
    if (session) {
      this.activeSessionTitle = session.title;
    }

    this.isLoadingMessages = true;
    this.errorMessage = '';
    this.isSidebarOpen = false;
    this.messages.set([]);

    this.chatService.getSessionById(sessionId).subscribe({
      next: (res) => {
        this.isLoadingMessages = false;
        if (res.isSuccess && res.data) {
          this.messages.set(res.data.messages ?? []);
          this.activeSessionTitle = res.data.title;
          this.shouldScrollToBottom = true;
          this.cdr.detectChanges();
        } else {
          this.errorMessage = res.message || 'Failed to load conversation history.';
        }
      },
      error: (err) => {
        this.isLoadingMessages = false;
        this.errorMessage = err.error?.message || err.message || 'Failed to load conversation history.';
        this.cdr.detectChanges();
      }
    });
  }

  startNewChat(): void {
    this.cancelTypewriter();
    this.voiceService.stopSpeaking();
    this.activeSessionId = null;
    this.activeSessionTitle = 'New Lesson';
    this.messages.set([]);
    this.isLoadingMessages = false;
    this.errorMessage = '';
    this.inputText = '';
    this.adjustTextareaHeight();
    this.isSidebarOpen = false;
    this.cdr.detectChanges();

    setTimeout(() => {
      this.messageInputRef?.nativeElement?.focus();
    }, 50);
  }

  deleteSession(event: Event, sessionId: number): void {
    event.stopPropagation();
    if (!confirm('Are you sure you want to delete this conversation?')) {
      return;
    }

    const previousSessions = [...this.sessions()];

    // 🚀 Optimistic Instant Deletion: Remove immediately from UI
    this.sessions.update(list => list.filter(s => s.id !== sessionId));

    if (this.activeSessionId === sessionId) {
      const remaining = this.sessions();
      if (remaining.length > 0) {
        this.selectSession(remaining[0].id);
      } else {
        this.startNewChat();
      }
    }
    this.cdr.detectChanges();

    // Perform background API call
    this.chatService.deleteSession(sessionId).subscribe({
      next: (res) => {
        if (!res.isSuccess) {
          // Rollback on server error
          this.sessions.set(previousSessions);
          this.errorMessage = res.message || 'Failed to delete lesson.';
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        // Rollback on network error
        this.sessions.set(previousSessions);
        this.errorMessage = err.error?.message || 'Failed to delete lesson.';
        this.cdr.detectChanges();
      }
    });
  }

  toggleMode(mode: AnswerMode): void {
    this.selectedMode = mode;
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  useTrackPrompt(track: PromptTrack): void {
    this.selectedMode = track.mode;
    this.sendMessage(track.examplePrompt, track.mode);
  }

  sendMessage(presetText?: string, presetMode?: AnswerMode): void {
    const textToSend = (presetText || this.inputText).trim();
    const modeToSend = presetMode !== undefined ? presetMode : this.selectedMode;

    if (!textToSend || this.isSending) return;

    if (textToSend.length > 500) {
      this.errorMessage = 'Questions must be within 500 characters.';
      return;
    }

    // Stop voice listening if currently recording
    this.voiceService.stopListening();

    if (!this.activeSessionId) {
      // Auto-title session with first 40 chars
      const sessionTitle = textToSend.length > 35 ? textToSend.substring(0, 35) + '...' : textToSend;
      this.chatService.createSession(sessionTitle).subscribe({
        next: (res) => {
          if (res.isSuccess && res.data) {
            const newSession: SessionSummary = {
              id: res.data.id,
              title: res.data.title,
              createdAt: res.data.createdAt,
              messageCount: 0
            };
            this.sessions.update(list => [newSession, ...list]);
            this.activeSessionId = newSession.id;
            this.activeSessionTitle = newSession.title;
            this.cdr.detectChanges();
            this.executeSend(this.activeSessionId, textToSend, modeToSend);
          }
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Could not start lesson session.';
        }
      });
      return;
    }

    this.executeSend(this.activeSessionId, textToSend, modeToSend);
  }

  private executeSend(sessionId: number, messageText: string, mode: AnswerMode): void {
    // Optimistic student message in UI
    const tempStudentMsg: ChatMessage = {
      id: Date.now(),
      role: MessageRole.Student,
      content: messageText,
      answerMode: mode,
      createdAt: new Date().toISOString()
    };

    this.messages.update(msgs => [...msgs, tempStudentMsg]);
    this.inputText = '';
    this.adjustTextareaHeight();
    this.isSending = true;
    this.errorMessage = '';
    this.shouldScrollToBottom = true;
    this.cdr.detectChanges();

    this.chatService.sendMessage(sessionId, messageText, mode).subscribe({
      next: (res) => {
        this.isSending = false;
        if (res.isSuccess && res.data) {
          const fullReply = res.data.assistantReply;
          const assistantMsg: ChatMessage = {
            id: res.data.assistantMessageId,
            role: MessageRole.Assistant,
            content: '',
            answerMode: res.data.answerMode,
            createdAt: res.data.createdAt
          };

          this.messages.update(msgs => {
            const updated = msgs.map(m =>
              m.id === tempStudentMsg.id
                ? { ...m, id: res.data!.userMessageId }
                : m
            );
            return [...updated, assistantMsg];
          });

          // Stream live response with crisp typewriter effect
          this.streamTypewriter(assistantMsg.id, fullReply);

          this.sessions.update(list =>
            list.map(s => s.id === sessionId ? { ...s, messageCount: s.messageCount + 2 } : s)
          );
          this.cdr.detectChanges();
        } else {
          this.errorMessage = res.message || 'Failed to receive tutor response.';
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSending = false;
        this.errorMessage = err.error?.message || 'Unable to connect to tutor service.';
        this.cdr.detectChanges();
      }
    });
  }

  finishTypewriterImmediately(messageId: number): void {
    if (this.typingMessageId === messageId && this.typewriterInterval) {
      this.cancelTypewriter();
      if (this.currentFullText) {
        this.messages.update(msgs =>
          msgs.map(m => (m.id === messageId ? { ...m, content: this.currentFullText } : m))
        );
      }
      this.shouldScrollToBottom = true;
      this.cdr.detectChanges();
    }
  }

  private streamTypewriter(messageId: number, fullText: string): void {
    this.cancelTypewriter();
    this.typingMessageId = messageId;
    this.currentFullText = fullText;

    const tokens = fullText.match(/\S+|\s+/g) || [fullText];
    let currentIndex = 0;
    let currentText = '';
    const totalTokens = tokens.length;

    const tokensPerTick = totalTokens > 300 ? 6 : (totalTokens > 150 ? 4 : 2);
    const tickIntervalMs = 16;

    this.typewriterInterval = setInterval(() => {
      if (currentIndex >= totalTokens) {
        this.cancelTypewriter();

        this.messages.update(msgs =>
          msgs.map(m => (m.id === messageId ? { ...m, content: fullText } : m))
        );
        this.shouldScrollToBottom = true;
        this.cdr.detectChanges();
        return;
      }

      const nextChunk = tokens.slice(currentIndex, currentIndex + tokensPerTick).join('');
      currentText += nextChunk;
      currentIndex += tokensPerTick;

      this.messages.update(msgs =>
        msgs.map(m => (m.id === messageId ? { ...m, content: currentText } : m))
      );

      this.shouldScrollToBottom = true;
      this.cdr.detectChanges();
    }, tickIntervalMs);
  }

  private cancelTypewriter(): void {
    if (this.typewriterInterval) {
      clearInterval(this.typewriterInterval);
      this.typewriterInterval = null;
    }
    this.typingMessageId = null;
  }

  // Voice Interaction Handlers
  toggleVoiceRecording(): void {
    if (this.voiceService.voiceState() === 'recording') {
      this.voiceService.stopListening();
    } else {
      this.voiceService.startListening((transcript: string, isFinal: boolean) => {
        this.inputText = transcript;
        this.adjustTextareaHeight();
        this.cdr.detectChanges();
        if (isFinal) {
          // Focus input so student can review or press send
          this.messageInputRef?.nativeElement?.focus();
        }
      });
    }
  }

  speakMessage(messageId: number, content: string): void {
    this.voiceService.speakText(messageId, content);
  }

  copyContent(messageId: number, content: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.copiedMessageId = messageId;
      this.cdr.detectChanges();
      setTimeout(() => {
        if (this.copiedMessageId === messageId) {
          this.copiedMessageId = null;
          this.cdr.detectChanges();
        }
      }, 2000);
    });
  }

  onTextInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.adjustTextareaHeight(textarea);
  }

  private adjustTextareaHeight(textarea?: HTMLTextAreaElement): void {
    const target = textarea || this.messageInputRef?.nativeElement;
    if (!target) return;
    target.style.height = 'auto';
    const newHeight = Math.min(target.scrollHeight, 180);
    target.style.height = `${newHeight}px`;
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  logout(): void {
    this.voiceService.stopSpeaking();
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        this.messagesContainer.nativeElement.scrollTop =
          this.messagesContainer.nativeElement.scrollHeight;
      }
    } catch {
      // scroll gracefully
    }
  }
}
