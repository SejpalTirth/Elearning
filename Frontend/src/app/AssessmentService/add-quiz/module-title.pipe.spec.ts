import { ModuleTitlePipe } from './module-title.pipe';

describe('ModuleTitlePipe', () => {

  let pipe: ModuleTitlePipe;

  beforeEach(() => {
    pipe = new ModuleTitlePipe();
  });

  // ---------------------------------------------------------
  it('should create the pipe', () => {
    expect(pipe).toBeTruthy();
  });

  // ---------------------------------------------------------
  it('should return empty string when modules is null', () => {
    expect(pipe.transform(null as any, 1)).toBe('');
  });

  // ---------------------------------------------------------
  it('should return empty string when id is null', () => {
    expect(pipe.transform([], null)).toBe('');
  });

  // ---------------------------------------------------------
  it('should return matching module title', () => {
    const modules = [
      { id: 1, title: 'Intro' },
      { id: 2, title: 'Basics' }
    ];

    expect(pipe.transform(modules, 2)).toBe('Basics');
  });

  // ---------------------------------------------------------
  it('should return empty string if module id not found', () => {
    const modules = [
      { id: 1, title: 'Intro' }
    ];

    expect(pipe.transform(modules, 999)).toBe('');
  });

});
