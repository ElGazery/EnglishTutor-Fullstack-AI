export enum MessageRole {
  Student = 0,
  Assistant = 1
}

export enum AnswerMode {
  Quick = 0,
  Detailed = 1
}

export interface SessionSummary {
  id: number;
  title: string;
  createdAt: string;
  messageCount: number;
}

export interface ChatMessage {
  id: number;
  role: MessageRole;
  content: string;
  answerMode: AnswerMode;
  createdAt: string;
}

export interface SessionDetail {
  id: number;
  title: string;
  createdAt: string;
  messages: ChatMessage[];
}

export interface SendMessageRequest {
  message: string;
  answerMode: AnswerMode;
}

export interface SendMessageResponse {
  userMessageId: number;
  assistantMessageId: number;
  assistantReply: string;
  answerMode: AnswerMode;
  createdAt: string;
}
