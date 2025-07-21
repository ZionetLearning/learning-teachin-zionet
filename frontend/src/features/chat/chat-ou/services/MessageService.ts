import type {
  MessageService,
  Message,
  MessageContext,
  MessageSender,
  TextMessage,
} from "../types";
import { MockChatService } from "./MockAIService";

export class MessageServiceImpl implements MessageService {
  private messages: Message[] = [];
  private subscribers: ((messages: Message[]) => void)[] = [];
  private chatService: MockChatService;

  private readonly userSender: MessageSender = {
    id: "user-1",
    name: "You",
    type: "user",
    avatar: "👤",
  };

  constructor() {
    this.chatService = new MockChatService();

    // Initialize with sample conversation
    this.initializeWithSampleData();
  }

  async sendMessage(content: string, context?: MessageContext): Promise<void> {
    // Create user message
    const userMessage: TextMessage = {
      id: `user-${Date.now()}`,
      type: "text",
      content,
      timestamp: new Date(),
      sender: this.userSender,
      context,
    };

    // Add user message to the conversation
    this.addMessage(userMessage);

    try {
      // Generate AI response
      const aiResponse = await this.chatService.generateResponse(
        content,
        context
      );

      // Add AI response to the conversation
      this.addMessage(aiResponse);
    } catch (error) {
      // Handle error by adding an error message
      const errorMessage: TextMessage = {
        id: `error-${Date.now()}`,
        type: "text",
        content:
          "Sorry, I encountered an error while processing your message. Please try again.",
        timestamp: new Date(),
        sender: {
          id: "system",
          name: "System",
          type: "ai",
          avatar: "⚠️",
        },
      };

      this.addMessage(errorMessage);
    }
  }

  getMessages(): Message[] {
    return [...this.messages];
  }

  subscribeToMessages(callback: (messages: Message[]) => void): () => void {
    this.subscribers.push(callback);

    // Immediately call with current messages
    callback(this.getMessages());

    // Return unsubscribe function
    return () => {
      const index = this.subscribers.indexOf(callback);
      if (index > -1) {
        this.subscribers.splice(index, 1);
      }
    };
  }

  private addMessage(message: Message): void {
    this.messages.push(message);
    this.notifySubscribers();
  }

  private notifySubscribers(): void {
    const currentMessages = this.getMessages();
    this.subscribers.forEach((callback) => callback(currentMessages));
  }

  private initializeWithSampleData(): void {
    const initialConversation = this.chatService.getInitialConversation();
    this.messages = [...initialConversation.messages];
  }

  // Additional utility methods for message management
  public clearMessages(): void {
    this.messages = [];
    this.notifySubscribers();
  }

  public getMessageById(id: string): Message | undefined {
    return this.messages.find((message) => message.id === id);
  }

  public getMessagesByType(type: Message["type"]): Message[] {
    return this.messages.filter((message) => message.type === type);
  }

  public getMessagesBySender(senderId: string): Message[] {
    return this.messages.filter((message) => message.sender.id === senderId);
  }
}
