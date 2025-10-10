export type ChatMessageRequest = {
  userInput: string;
  userId: string;
  userName: string;
};

export type ChatMessage = {
  message: string;
  sender: 'User' | 'AI';
};
