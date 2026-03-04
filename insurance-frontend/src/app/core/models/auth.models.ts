export interface AuthResponseDto {
  token: string;
  email: string;
  role: string;
}

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface RegisterRequestDto {
  name: string;
  email: string;
  password: string;
  phoneNumber: string;
  role: string;
}
