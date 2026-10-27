import { Injectable, signal } from '@angular/core';

export type VoiceState = 'idle' | 'recording' | 'processing' | 'speaking' | 'error';

@Injectable({
  providedIn: 'root'
})
export class VoiceService {
  // Reactive voice states
  public readonly voiceState = signal<VoiceState>('idle');
  public readonly activeSpeakingId = signal<number | null>(null);
  public readonly errorMessage = signal<string>('');

  private recognition: any = null;
  private currentUtterance: SpeechSynthesisUtterance | null = null;
  public readonly isRecognitionSupported: boolean;
  public readonly isSynthesisSupported: boolean;

  constructor() {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    this.isRecognitionSupported = !!SpeechRecognition;
    this.isSynthesisSupported = typeof window !== 'undefined' && 'speechSynthesis' in window;

    if (this.isRecognitionSupported) {
      try {
        this.recognition = new SpeechRecognition();
        this.recognition.continuous = false;
        this.recognition.interimResults = true;
        this.recognition.lang = 'en-US';
      } catch (e) {
        console.warn('SpeechRecognition initialization error:', e);
      }
    }
  }

  /**
   * Start listening to student's voice input
   */
  startListening(onTranscript: (text: string, isFinal: boolean) => void): void {
    if (!this.isRecognitionSupported || !this.recognition) {
      this.voiceState.set('error');
      this.errorMessage.set('Voice recognition is not supported in this browser. Please use Chrome or Edge.');
      return;
    }

    // Stop speaking if tutor is currently talking
    this.stopSpeaking();

    this.errorMessage.set('');
    this.voiceState.set('recording');

    this.recognition.onresult = (event: any) => {
      let interimTranscript = '';
      let finalTranscript = '';

      for (let i = event.resultIndex; i < event.results.length; ++i) {
        if (event.results[i].isFinal) {
          finalTranscript += event.results[i][0].transcript;
        } else {
          interimTranscript += event.results[i][0].transcript;
        }
      }

      if (finalTranscript) {
        this.voiceState.set('processing');
        onTranscript(finalTranscript.trim(), true);
      } else if (interimTranscript) {
        onTranscript(interimTranscript.trim(), false);
      }
    };

    this.recognition.onerror = (event: any) => {
      console.warn('Speech recognition error:', event.error);
      this.voiceState.set('error');
      if (event.error === 'not-allowed') {
        this.errorMessage.set('Microphone access denied. Please allow microphone permissions.');
      } else if (event.error === 'no-speech') {
        this.errorMessage.set('No speech detected. Please speak clearly into your microphone.');
      } else {
        this.errorMessage.set(`Voice error: ${event.error}`);
      }

      setTimeout(() => {
        if (this.voiceState() === 'error') {
          this.voiceState.set('idle');
        }
      }, 3500);
    };

    this.recognition.onend = () => {
      if (this.voiceState() === 'recording' || this.voiceState() === 'processing') {
        this.voiceState.set('idle');
      }
    };

    try {
      this.recognition.start();
    } catch (e) {
      console.warn('Recognition start exception:', e);
      this.voiceState.set('idle');
    }
  }

  /**
   * Stop recording student's speech
   */
  stopListening(): void {
    if (this.recognition && this.voiceState() === 'recording') {
      try {
        this.recognition.stop();
      } catch (e) {
        console.warn('Recognition stop exception:', e);
      }
    }
    this.voiceState.set('idle');
  }

  /**
   * Text-to-Speech: Read aloud the English tutor's explanation with native pronunciation
   */
  speakText(messageId: number, rawMarkdownText: string): void {
    if (!this.isSynthesisSupported) {
      this.voiceState.set('error');
      this.errorMessage.set('Speech synthesis is not supported on your device.');
      return;
    }

    // If currently speaking this message, toggle pause/stop
    if (this.activeSpeakingId() === messageId) {
      this.stopSpeaking();
      return;
    }

    // Stop any ongoing speech
    this.stopSpeaking();

    // Clean markdown characters for pleasant pronunciation
    const cleanText = this.stripMarkdownForSpeech(rawMarkdownText);
    if (!cleanText.trim()) return;

    this.currentUtterance = new SpeechSynthesisUtterance(cleanText);
    this.currentUtterance.lang = 'en-US';
    this.currentUtterance.rate = 0.95; // Slightly slower, friendly teaching speed for prep students
    this.currentUtterance.pitch = 1.0;

    // Select an English voice if available
    const voices = window.speechSynthesis.getVoices();
    const englishVoice = voices.find(v => v.lang.startsWith('en') && (v.name.includes('Natural') || v.name.includes('Google') || v.name.includes('Jenny') || v.name.includes('David')));
    if (englishVoice) {
      this.currentUtterance.voice = englishVoice;
    }

    this.activeSpeakingId.set(messageId);
    this.voiceState.set('speaking');

    this.currentUtterance.onend = () => {
      this.activeSpeakingId.set(null);
      if (this.voiceState() === 'speaking') {
        this.voiceState.set('idle');
      }
    };

    this.currentUtterance.onerror = (e) => {
      console.warn('Speech synthesis error:', e);
      this.activeSpeakingId.set(null);
      this.voiceState.set('idle');
    };

    window.speechSynthesis.speak(this.currentUtterance);
  }

  /**
   * Stop active speech playback
   */
  stopSpeaking(): void {
    if (this.isSynthesisSupported) {
      window.speechSynthesis.cancel();
    }
    this.activeSpeakingId.set(null);
    if (this.voiceState() === 'speaking') {
      this.voiceState.set('idle');
    }
  }

  /**
   * Remove markdown formatting symbols for natural audio speech
   */
  private stripMarkdownForSpeech(text: string): string {
    return text
      .replace(/```[\s\S]*?```/g, ' Code snippet omitted. ')
      .replace(/`([^`]+)`/g, '$1')
      .replace(/\*\*([^*]+)\*\*/g, '$1')
      .replace(/\*([^*]+)\*/g, '$1')
      .replace(/#+\s+/g, '')
      .replace(/\[([^\]]+)\]\([^)]+\)/g, '$1')
      .replace(/^[*-]\s+/gm, '')
      .replace(/>\s+/g, '')
      .trim();
  }
}
